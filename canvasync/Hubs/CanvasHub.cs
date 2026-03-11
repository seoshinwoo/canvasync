using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using canvasync.Library.Models;
using canvasync.Library.Dtos;
using canvasync.Containers;
using canvasync.Library.Services;
using canvasync.Services;

namespace Hubs;

public class CanvasHub : Hub
{
    private readonly StateContainer _stateContainer;
    private readonly ICanvasService _canvasService;
    private readonly IDrawingStorageService _drawingStorage;

    // 페이지 단위 동시 수정 방지: "lectureId:pageIndex" → SemaphoreSlim(1,1)
    // static이므로 Hub 인스턴스가 매 요청마다 새로 생성되어도 잠금 상태가 공유됨
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _pageLocks = new();

    public CanvasHub(StateContainer stateContainer, ICanvasService canvasService, IDrawingStorageService drawingStorage)
    {
        _stateContainer = stateContainer;
        _canvasService = canvasService;
        _drawingStorage = drawingStorage;
    }

    public override async Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;

        var httpContext = Context.GetHttpContext();

        var lectureId = httpContext?.Request.Query["lectureId"].ToString();
        var memberId = httpContext?.Request.Query["memberId"].ToString();

        if (!string.IsNullOrEmpty(lectureId))
        {
            var lecture = await _canvasService.GetLectureAsync(lectureId);
            bool isHost = lecture?.HostMember?.Id == memberId;

            // Redis SET에 접속자 등록 + connectionId → lectureId 매핑 저장
            await _drawingStorage.AddMemberAsync(lectureId, connectionId, isHost, memberId ?? string.Empty);
            await _drawingStorage.SetConnectionMappingAsync(connectionId, lectureId);

            if (!await _drawingStorage.ContainsKeyAsync(lectureId))
            {
                var drawingData = await _canvasService.GetDrawingDataAsync(lectureId, memberId);

                if (drawingData != null)
                {
                    await _drawingStorage.SetAsync(lectureId, drawingData.Drawings);
                }
                else
                {
                    await _drawingStorage.SetAsync(lectureId, new List<List<FactorDto>>());
                }
            }
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;

        // Redis에서 connectionId → lectureId 조회
        var lectureId = await _drawingStorage.GetLectureIdByConnectionAsync(connectionId);
        if (lectureId is null) return;

        // 해당 사용자 정보 조회
        var memberInfo = await _drawingStorage.GetMemberInfoAsync(lectureId, connectionId);
        if (memberInfo is null) return;

        var (isHost, memberId) = memberInfo.Value;

        // Redis SET에서 접속자 제거 + 매핑 삭제
        await _drawingStorage.RemoveMemberAsync(lectureId, connectionId);
        await _drawingStorage.RemoveConnectionMappingAsync(connectionId);

        if (isHost)
        {
            // Host가 나갔을 때: 현재 Redis 데이터를 DB에 즉시 저장
            var drawings = await _drawingStorage.GetAsync(lectureId);
            if (drawings != null)
            {
                await _canvasService.SaveDrawingDataAsync(new DrawingData
                {
                    LectureId = lectureId,
                    MemberId = memberId,
                    Drawings = drawings
                });
                Console.WriteLine($"Host 퇴장 → 강의 [{lectureId}] 드로잉 데이터 DB 저장 완료");
            }

            // Redis 데이터는 Guest용으로 2시간 더 유지 후 자동 만료
            await _drawingStorage.SetExpiryAsync(lectureId, TimeSpan.FromHours(2));
            // 히스토리/Redo 스택 정리
            await _drawingStorage.ClearHistoryAsync(lectureId);
            Console.WriteLine($"Host 퇴장 → 강의 [{lectureId}] Redis 데이터 2시간 후 자동 삭제 예정");
        }
        // Guest가 나갔을 때는 Redis 데이터를 건드리지 않음
    }
    
    public async Task SendDrawings(string user, FactorData factorData)
    {
        Console.WriteLine($"Host가 Guest에게 {factorData.FactorDto.FactorType}을 방송");

        // 브로드캐스트(A)와 상태 저장(B)은 서로 독립적 → 병렬 실행
        // A: 클라이언트 브로드캐스트는 factorData(불변 DTO)만 사용하므로 저장 완료를 기다릴 필요 없음
        // B: 페이지 상태 변경 + Redis 저장은 별도 파이프라인
        await Task.WhenAll(
            Clients.Others.SendAsync("ReceiveDrawings", user, factorData),
            PersistDrawingAsync(factorData)
        );
    }

    /// <summary>
    /// 드로잉 액션을 페이지에 반영하고 저장합니다.
    /// SemaphoreSlim으로 동일 페이지의 동시 수정(Race Condition)을 방지합니다.
    /// </summary>
    private async Task PersistDrawingAsync(FactorData factorData)
    {
        var lockKey = $"{factorData.LectureId}:{factorData.PageIndex}";
        var sem = _pageLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await sem.WaitAsync();
        try
        {
            // 페이지 확보 + 조회 (순차 - EnsurePage → GetPage 의존 관계)
            await _drawingStorage.EnsurePageCountAsync(factorData.LectureId, factorData.PageIndex + 1);

            var page = await _drawingStorage.GetPageAsync(factorData.LectureId, factorData.PageIndex)
                       ?? new List<FactorDto>();

            switch (factorData.FactorAction)
            {
                case "Add":    page.Add(factorData.FactorDto); break;
                case "Delete": page.RemoveAt(factorData.FactorIndex); break;
                case "Update":
                case "End":    page[factorData.FactorIndex] = factorData.FactorDto; break;
            }

            // 저장, 히스토리 기록, Redo 초기화는 서로 독립적 → 병렬 실행
            await Task.WhenAll(
                _drawingStorage.SetPageAsync(factorData.LectureId, factorData.PageIndex, page),
                _drawingStorage.PushHistoryAsync(factorData.LectureId, factorData.PageIndex, factorData),
                _drawingStorage.ClearRedoAsync(factorData.LectureId, factorData.PageIndex)
            );
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task UndoDrawing(string lectureId, int pageIndex)
    {
        var lockKey = $"{lectureId}:{pageIndex}";
        var sem = _pageLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await sem.WaitAsync();
        try
        {
            var lastAction = await _drawingStorage.PopHistoryAsync(lectureId, pageIndex);
            if (lastAction is null) return;

            var page = await _drawingStorage.GetPageAsync(lectureId, pageIndex)
                       ?? new List<FactorDto>();

            switch (lastAction.FactorAction)
            {
                case "Add":
                    if (page.Count > 0)
                        page.RemoveAt(page.Count - 1);
                    break;
                case "Delete":
                    if (lastAction.FactorIndex <= page.Count)
                        page.Insert(lastAction.FactorIndex, lastAction.FactorDto);
                    break;
                case "Update":
                case "End":
                    break;
            }

            // 저장 + Redo 기록은 독립 → 병렬 실행
            await Task.WhenAll(
                _drawingStorage.SetPageAsync(lectureId, pageIndex, page),
                _drawingStorage.PushRedoAsync(lectureId, pageIndex, lastAction)
            );

            // lock 안에서 브로드캐스트: page 참조가 다른 스레드에 의해 변경되지 않음을 보장
            await Clients.All.SendAsync("PageRefreshed", lectureId, pageIndex, page);
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task RedoDrawing(string lectureId, int pageIndex)
    {
        var lockKey = $"{lectureId}:{pageIndex}";
        var sem = _pageLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await sem.WaitAsync();
        try
        {
            var redoAction = await _drawingStorage.PopRedoAsync(lectureId, pageIndex);
            if (redoAction is null) return;

            var page = await _drawingStorage.GetPageAsync(lectureId, pageIndex)
                       ?? new List<FactorDto>();

            switch (redoAction.FactorAction)
            {
                case "Add":
                    page.Add(redoAction.FactorDto);
                    break;
                case "Delete":
                    if (redoAction.FactorIndex < page.Count)
                        page.RemoveAt(redoAction.FactorIndex);
                    break;
                case "Update":
                case "End":
                    if (redoAction.FactorIndex < page.Count)
                        page[redoAction.FactorIndex] = redoAction.FactorDto;
                    break;
            }

            // 저장 + 히스토리 기록은 독립 → 병렬 실행
            await Task.WhenAll(
                _drawingStorage.SetPageAsync(lectureId, pageIndex, page),
                _drawingStorage.PushHistoryAsync(lectureId, pageIndex, redoAction)
            );

            await Clients.All.SendAsync("PageRefreshed", lectureId, pageIndex, page);
        }
        finally
        {
            sem.Release();
        }
    }
}