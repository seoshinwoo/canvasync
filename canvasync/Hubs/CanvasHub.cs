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
        await Clients.Others.SendAsync("ReceiveDrawings", user, factorData);

        // 필요한 페이지가 아직 없으면 빈 페이지 생성
        await _drawingStorage.EnsurePageCountAsync(factorData.LectureId, factorData.PageIndex + 1);

        // 해당 페이지만 조회
        var page = await _drawingStorage.GetPageAsync(factorData.LectureId, factorData.PageIndex)
                   ?? new List<FactorDto>();

        switch (factorData.FactorAction)
        {
            case "Add":
                page.Add(factorData.FactorDto);
                break;
            case "Delete":
                page.RemoveAt(factorData.FactorIndex);
                break;
            case "Update":
            case "End":
                page[factorData.FactorIndex] = factorData.FactorDto;
                break;
        }

        // 해당 페이지만 저장
        await _drawingStorage.SetPageAsync(factorData.LectureId, factorData.PageIndex, page);

        // 히스토리에 액션 기록 (Undo용) + Redo 스택 초기화
        await _drawingStorage.PushHistoryAsync(factorData.LectureId, factorData.PageIndex, factorData);
        await _drawingStorage.ClearRedoAsync(factorData.LectureId, factorData.PageIndex);
    }

    public async Task UndoDrawing(string lectureId, int pageIndex)
    {
        // 히스토리에서 마지막 액션 꺼내기
        var lastAction = await _drawingStorage.PopHistoryAsync(lectureId, pageIndex);
        if (lastAction is null) return;

        // 해당 페이지 조회
        var page = await _drawingStorage.GetPageAsync(lectureId, pageIndex)
                   ?? new List<FactorDto>();

        // 액션의 역상 적용
        switch (lastAction.FactorAction)
        {
            case "Add":
                // 추가의 역 → 마지막 요소 제거
                if (page.Count > 0)
                    page.RemoveAt(page.Count - 1);
                break;
            case "Delete":
                // 삭제의 역 → 해당 위치에 복원
                if (lastAction.FactorIndex <= page.Count)
                    page.Insert(lastAction.FactorIndex, lastAction.FactorDto);
                break;
            case "Update":
            case "End":
                // 업데이트의 역은 Redo에서 현재 상태를 기록 후 복원
                // (원본 상태를 모르므로 페이지 전체를 Redo에 저장)
                break;
        }

        await _drawingStorage.SetPageAsync(lectureId, pageIndex, page);
        await _drawingStorage.PushRedoAsync(lectureId, pageIndex, lastAction);

        // 모든 클라이언트에게 페이지 갱신 알림
        await Clients.All.SendAsync("PageRefreshed", lectureId, pageIndex, page);
    }

    public async Task RedoDrawing(string lectureId, int pageIndex)
    {
        // Redo 스택에서 마지막 액션 꺼내기
        var redoAction = await _drawingStorage.PopRedoAsync(lectureId, pageIndex);
        if (redoAction is null) return;

        // 해당 페이지 조회
        var page = await _drawingStorage.GetPageAsync(lectureId, pageIndex)
                   ?? new List<FactorDto>();

        // 원래 액션을 다시 적용
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

        await _drawingStorage.SetPageAsync(lectureId, pageIndex, page);
        await _drawingStorage.PushHistoryAsync(lectureId, pageIndex, redoAction);

        // 모든 클라이언트에게 페이지 갱신 알림
        await Clients.All.SendAsync("PageRefreshed", lectureId, pageIndex, page);
    }
}