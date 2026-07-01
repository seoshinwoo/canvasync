using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using canvasync.Library.Models;
using canvasync.Library.Dtos;
using canvasync.Containers;
using canvasync.Library.Services;
using canvasync.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Hubs;

[Authorize]
public class CanvasHub : Hub
{
    private const string LectureGroupPrefix = "lecture:";
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

    private static string GetLectureGroupName(string lectureId) => $"{LectureGroupPrefix}{lectureId}";

    public override async Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;

        var httpContext = Context.GetHttpContext();

        var lectureId = httpContext?.Request.Query["lectureId"].ToString();
        var queryMemberId = httpContext?.Request.Query["memberId"].ToString();
        var memberId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? queryMemberId;

        if (!string.IsNullOrEmpty(lectureId) && !string.IsNullOrEmpty(memberId))
        {
            if (!await _canvasService.CanAccessLectureAsync(lectureId, memberId))
            {
                Context.Abort();
                return;
            }

            await Groups.AddToGroupAsync(connectionId, GetLectureGroupName(lectureId));

            var lecture = await _canvasService.GetLectureAsync(lectureId);
            bool isHost = lecture?.HostMember?.Id == memberId;

            // Redis SET에 접속자 등록 + connectionId → lectureId 매핑 저장
            await _drawingStorage.AddMemberAsync(lectureId, connectionId, isHost, memberId);
            await _drawingStorage.SetConnectionMappingAsync(connectionId, lectureId);

            if (!await _drawingStorage.ContainsKeyAsync(lectureId))
            {
                var hostMemberId = lecture?.HostMember?.Id;
                var drawingData = string.IsNullOrEmpty(hostMemberId)
                    ? null
                    : await _canvasService.GetDrawingDataAsync(lectureId, hostMemberId);

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

        await Groups.RemoveFromGroupAsync(connectionId, GetLectureGroupName(lectureId));

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
            Console.WriteLine($"Host 퇴장 → 강의 [{lectureId}] Redis 데이터 2시간 후 자동 삭제 예정");
        }
        // Guest가 나갔을 때는 Redis 데이터를 건드리지 않음
    }
    
    public async Task SendDrawings(string user, FactorData factorData)
    {
        Console.WriteLine($"Host가 Guest에게 {factorData.FactorDto.FactorType}을 방송");

        var lectureGroupName = GetLectureGroupName(factorData.LectureId);

        // 브로드캐스트(A)와 상태 저장(B)은 서로 독립적 → 병렬 실행
        // A: 클라이언트 브로드캐스트는 factorData(불변 DTO)만 사용하므로 저장 완료를 기다릴 필요 없음
        // B: 페이지 상태 변경 + Redis 저장은 별도 파이프라인
        await Task.WhenAll(
            Clients.OthersInGroup(lectureGroupName).SendAsync("ReceiveDrawings", user, factorData),
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
            var drawings = await _drawingStorage.GetAsync(factorData.LectureId)
                           ?? new List<List<FactorDto>>();

            // 필요한 페이지 수만큼 확장
            while (drawings.Count <= factorData.PageIndex)
            {
                drawings.Add(new List<FactorDto>());
            }

            var page = drawings[factorData.PageIndex];
            switch (factorData.FactorAction)
            {
                case "Add":    page.Add(factorData.FactorDto); break;
                case "Delete": page.RemoveAt(factorData.FactorIndex); break;
                case "Update":
                case "End":    page[factorData.FactorIndex] = factorData.FactorDto; break;
            }

            await _drawingStorage.SetAsync(factorData.LectureId, drawings);
        }
        finally
        {
            sem.Release();
        }
    }

}
