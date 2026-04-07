

using System.Collections.Concurrent;
using canvasync.Library.Dtos;

namespace canvasync.Services;

public class InMemoryDrawingStorageService : IDrawingStorageService
{
    public record DrawingInfo(bool IsHost, string MemberId, List<List<FactorDto>> Drawings);
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, DrawingInfo>> _drawingStorage = new();
    public Task AddMemberAsync(string lectureId, string connectionId, bool isHost, string memberId)
    {
        var drawingInfo = new DrawingInfo(isHost, memberId, new List<List<FactorDto>>());
        
        _drawingStorage.TryAdd(lectureId, drawingInfo);

        return Task.CompletedTask;
    }

    public Task<bool> ContainsKeyAsync(string lectureId)
    {
        return Task.FromResult(_drawingStorage.ContainsKey(lectureId));
    }

    public Task<List<List<FactorDto>>?> GetAsync(string lectureId)
    {
        var value = _drawingStorage.

        return Task.FromResult();
    }

    public Task<string?> GetLectureIdByConnectionAsync(string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task<long> GetMemberCountAsync(string lectureId)
    {
        throw new NotImplementedException();
    }

    public Task<(bool IsHost, string MemberId)?> GetMemberInfoAsync(string lectureId, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(string lectureId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveConnectionMappingAsync(string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task RemoveMemberAsync(string lectureId, string connectionId)
    {
        throw new NotImplementedException();
    }

    public Task SetAsync(string lectureId, List<List<FactorDto>> drawings)
    {
        throw new NotImplementedException();
    }

    public Task SetConnectionMappingAsync(string connectionId, string lectureId)
    {
        throw new NotImplementedException();
    }

    public Task SetExpiryAsync(string lectureId, TimeSpan expiry)
    {
        throw new NotImplementedException();
    }
}