using System.Collections.Concurrent;
using canvasync.Library.Dtos;

namespace canvasync.Services;

public class InMemoryDrawingStorageService : IDrawingStorageService
{
    public record DrawingInfo(bool IsHost, string MemberId, List<List<FactorDto>> Drawings);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DrawingInfo>> _drawingStorage = new();
    private readonly ConcurrentDictionary<string, string> _connectionMapping = new(); // ConnectionId 관리 전용 딕셔너리 추가

    public Task AddMemberAsync(string lectureId, string connectionId, bool isHost, string memberId)
    {
        var drawingInfo = new DrawingInfo(isHost, memberId, new List<List<FactorDto>>());
        
        // 키가 없을 경우 자동으로 생성 후 추가 (기존 예외 발생 가능성 수정)
        var room = _drawingStorage.GetOrAdd(lectureId, _ => new ConcurrentDictionary<string, DrawingInfo>());
        room.TryAdd(connectionId, drawingInfo);

        return Task.CompletedTask;
    }

    public Task<bool> ContainsKeyAsync(string lectureId)
    {
        return Task.FromResult(_drawingStorage.ContainsKey(lectureId));
    }

    public Task<List<List<FactorDto>>?> GetAsync(string lectureId)
    {
        if (_drawingStorage.ContainsKey(lectureId))
        {
            var room = _drawingStorage[lectureId];
            var value = room.Where(r => r.Value.IsHost == true).Select(r => r.Value.Drawings).FirstOrDefault();
            return Task.FromResult(value);
        }

        return Task.FromResult<List<List<FactorDto>>?>(null);
    }

    public Task<string?> GetLectureIdByConnectionAsync(string connectionId)
    {
        // 최적화된 매핑 딕셔너리에서 즉각적으로 찾음
        _connectionMapping.TryGetValue(connectionId, out var lectureId);
        return Task.FromResult(lectureId);
    }

    public Task<long> GetMemberCountAsync(string lectureId)
    {
        if (_drawingStorage.TryGetValue(lectureId, out var room))
        {
            // room 안에 들어있는 원소가 connectionId 이므로 이 개수가 접속자 수가 됩니다.
            return Task.FromResult((long)room.Count);
        }
        return Task.FromResult(0L);
    }

    public Task<(bool IsHost, string MemberId)?> GetMemberInfoAsync(string lectureId, string connectionId)
    {
        if (_drawingStorage.TryGetValue(lectureId, out var room) && room.TryGetValue(connectionId, out var info))
        {
            return Task.FromResult<(bool IsHost, string MemberId)?>((info.IsHost, info.MemberId));
        }
        return Task.FromResult<(bool IsHost, string MemberId)?>(null);
    }

    public Task RemoveAsync(string lectureId)
    {
        _drawingStorage.TryRemove(lectureId, out _);
        return Task.CompletedTask;
    }

    public Task RemoveConnectionMappingAsync(string connectionId)
    {
        // 맵핑에서 해당 커넥션 삭제
        _connectionMapping.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }

    public Task RemoveMemberAsync(string lectureId, string connectionId)
    {
        if (_drawingStorage.TryGetValue(lectureId, out var room))
        {
            room.TryRemove(connectionId, out _);
            if (room.IsEmpty)
            {
                _drawingStorage.TryRemove(lectureId, out _);
            }
        }
        return Task.CompletedTask;
    }

    public Task SetAsync(string lectureId, List<List<FactorDto>> drawings)
    {
        // 방이 없으면 생성
        var room = _drawingStorage.GetOrAdd(lectureId, _ => new ConcurrentDictionary<string, DrawingInfo>());
        
        var host = room.FirstOrDefault(x => x.Value.IsHost);
        if (host.Key != null)
        {
            var newInfo = host.Value with { Drawings = drawings };
            room.TryUpdate(host.Key, newInfo, host.Value);
        }
        else
        {
            // 호스트가 아직 접속안했거나 방 데이터가 비어있을 경우, 그리기 데이터 보존을 위해 가상 호스트 데이터로 저장
            room.TryAdd("System_Host", new DrawingInfo(true, "System", drawings));
        }
        
        return Task.CompletedTask;
    }

    public Task SetConnectionMappingAsync(string connectionId, string lectureId)
    {
        // 커넥션과 렉처 ID를 저장
        _connectionMapping[connectionId] = lectureId;
        return Task.CompletedTask;
    }

    public Task SetExpiryAsync(string lectureId, TimeSpan expiry)
    {
        // 메모리 상에서 TTL이 다 될 경우 자동으로 제거되도록 비동기 지연 작업 세팅
        _ = Task.Delay(expiry).ContinueWith(_ => RemoveAsync(lectureId));
        return Task.CompletedTask;
    }
}