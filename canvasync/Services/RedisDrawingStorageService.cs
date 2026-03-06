using System.Text.Json;
using canvasync.Library.Dtos;
using Microsoft.Extensions.Caching.Distributed;

namespace canvasync.Services;

public class RedisDrawingStorageService : IDrawingStorageService
{
    private readonly IDistributedCache _cache;
    private const string KeyPrefix = "drawing:";

    public RedisDrawingStorageService(IDistributedCache cache)
    {
        _cache = cache;
    }

    private static string GetKey(string lectureId) => $"{KeyPrefix}{lectureId}";

    public async Task<bool> ContainsKeyAsync(string lectureId)
    {
        var data = await _cache.GetAsync(GetKey(lectureId));
        return data is not null;
    }

    public async Task<List<List<FactorDto>>?> GetAsync(string lectureId)
    {
        var data = await _cache.GetAsync(GetKey(lectureId));
        if (data is null) return null;

        return JsonSerializer.Deserialize<List<List<FactorDto>>>(data);
    }

    public async Task SetAsync(string lectureId, List<List<FactorDto>> drawings)
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(drawings);

        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = TimeSpan.FromHours(2)
        };

        await _cache.SetAsync(GetKey(lectureId), json, options);
    }

    public async Task RemoveAsync(string lectureId)
    {
        await _cache.RemoveAsync(GetKey(lectureId));
    }
}
