using System.Collections.Concurrent;
using canvasync.Library.Dtos;
using canvasync.Library.Models;

namespace canvasync.Services;

/// <summary>
/// L1(ConcurrentDictionary) + L2(Redis) 하이브리드 캐싱 서비스.
/// 드로잉 페이지 데이터는 인메모리 캐시를 우선 조회하여 Redis 왕복을 줄이고,
/// Redis는 영속성과 수평 확장(다중 서버 인스턴스 동기화)을 위한 L2로 사용합니다.
/// 접속자 관리, 연결 매핑, Undo/Redo 히스토리는 Redis에 직접 위임합니다.
/// </summary>
public class HybridDrawingStorageService : IDrawingStorageService
{
    private readonly RedisDrawingStorageService _redis;

    // L1 캐시: lectureId → (pageIndex → factors)
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<int, List<FactorDto>>> _pageCache = new();

    public HybridDrawingStorageService(RedisDrawingStorageService redis)
    {
        _redis = redis;
    }

    // ── 전체 페이지 (Hash 기반) ──

    public async Task<bool> ContainsKeyAsync(string lectureId)
    {
        // L1 히트
        if (_pageCache.ContainsKey(lectureId)) return true;
        // L1 미스 → L2
        return await _redis.ContainsKeyAsync(lectureId);
    }

    public async Task<List<List<FactorDto>>?> GetAsync(string lectureId)
    {
        // L1 히트
        if (_pageCache.TryGetValue(lectureId, out var pages) && pages.Count > 0)
        {
            var maxIndex = pages.Keys.Max();
            var result = new List<List<FactorDto>>();
            for (int i = 0; i <= maxIndex; i++)
            {
                result.Add(pages.TryGetValue(i, out var p) ? p : new List<FactorDto>());
            }
            return result;
        }

        // L1 미스 → L2 조회 후 L1에 캐시
        var fromRedis = await _redis.GetAsync(lectureId);
        if (fromRedis != null)
        {
            var dict = new ConcurrentDictionary<int, List<FactorDto>>();
            for (int i = 0; i < fromRedis.Count; i++)
            {
                dict[i] = fromRedis[i];
            }
            _pageCache[lectureId] = dict;
        }
        return fromRedis;
    }

    public async Task SetAsync(string lectureId, List<List<FactorDto>> drawings)
    {
        // L1 업데이트
        var dict = new ConcurrentDictionary<int, List<FactorDto>>();
        for (int i = 0; i < drawings.Count; i++)
        {
            dict[i] = drawings[i];
        }
        _pageCache[lectureId] = dict;

        // L2 write-through
        await _redis.SetAsync(lectureId, drawings);
    }

    public async Task RemoveAsync(string lectureId)
    {
        _pageCache.TryRemove(lectureId, out _);
        await _redis.RemoveAsync(lectureId);
    }

    public Task SetExpiryAsync(string lectureId, TimeSpan expiry)
    {
        // L1에는 TTL 개념이 없으므로 Redis만 설정.
        // L1 캐시는 RemoveAsync 호출 또는 서버 재시작 시 정리됨.
        // Guest가 아직 접속 중일 수 있으므로 L1 데이터는 유지.
        return _redis.SetExpiryAsync(lectureId, expiry);
    }

    // ── 페이지 단위 (Hash field) ──

    public async Task<List<FactorDto>?> GetPageAsync(string lectureId, int pageIndex)
    {
        // L1 히트
        if (_pageCache.TryGetValue(lectureId, out var pages) &&
            pages.TryGetValue(pageIndex, out var cached))
        {
            return cached;
        }

        // L1 미스 → L2 조회 후 L1에 캐시
        var fromRedis = await _redis.GetPageAsync(lectureId, pageIndex);
        if (fromRedis != null)
        {
            var dict = _pageCache.GetOrAdd(lectureId, _ => new ConcurrentDictionary<int, List<FactorDto>>());
            dict[pageIndex] = fromRedis;
        }
        return fromRedis;
    }

    public async Task SetPageAsync(string lectureId, int pageIndex, List<FactorDto> factors)
    {
        // L1 업데이트
        var dict = _pageCache.GetOrAdd(lectureId, _ => new ConcurrentDictionary<int, List<FactorDto>>());
        dict[pageIndex] = factors;

        // L2 write-through
        await _redis.SetPageAsync(lectureId, pageIndex, factors);
    }

    public async Task<int> GetPageCountAsync(string lectureId)
    {
        if (_pageCache.TryGetValue(lectureId, out var pages) && pages.Count > 0)
        {
            return pages.Count;
        }
        return await _redis.GetPageCountAsync(lectureId);
    }

    public async Task EnsurePageCountAsync(string lectureId, int requiredPageCount)
    {
        var dict = _pageCache.GetOrAdd(lectureId, _ => new ConcurrentDictionary<int, List<FactorDto>>());
        bool anyMissing = false;

        for (int i = 0; i < requiredPageCount; i++)
        {
            if (!dict.ContainsKey(i))
            {
                dict[i] = new List<FactorDto>();
                anyMissing = true;
            }
        }

        // L1에 없었던 페이지가 있을 때만 Redis에도 동기화
        if (anyMissing)
        {
            await _redis.EnsurePageCountAsync(lectureId, requiredPageCount);
        }
    }

    // ── 접속자 관리, 연결 매핑, 히스토리 → Redis 직접 위임 ──
    // 이 데이터들은 다중 서버 인스턴스 간 공유가 필요하고,
    // 요청 빈도가 낮아 Redis 왕복 비용이 문제되지 않음.

    public Task AddMemberAsync(string lectureId, string connectionId, bool isHost, string memberId)
        => _redis.AddMemberAsync(lectureId, connectionId, isHost, memberId);

    public Task RemoveMemberAsync(string lectureId, string connectionId)
        => _redis.RemoveMemberAsync(lectureId, connectionId);

    public Task<(bool IsHost, string MemberId)?> GetMemberInfoAsync(string lectureId, string connectionId)
        => _redis.GetMemberInfoAsync(lectureId, connectionId);

    public Task<long> GetMemberCountAsync(string lectureId)
        => _redis.GetMemberCountAsync(lectureId);

    public Task SetConnectionMappingAsync(string connectionId, string lectureId)
        => _redis.SetConnectionMappingAsync(connectionId, lectureId);

    public Task<string?> GetLectureIdByConnectionAsync(string connectionId)
        => _redis.GetLectureIdByConnectionAsync(connectionId);

    public Task RemoveConnectionMappingAsync(string connectionId)
        => _redis.RemoveConnectionMappingAsync(connectionId);

    public Task PushHistoryAsync(string lectureId, int pageIndex, FactorData action)
        => _redis.PushHistoryAsync(lectureId, pageIndex, action);

    public Task<FactorData?> PopHistoryAsync(string lectureId, int pageIndex)
        => _redis.PopHistoryAsync(lectureId, pageIndex);

    public Task PushRedoAsync(string lectureId, int pageIndex, FactorData action)
        => _redis.PushRedoAsync(lectureId, pageIndex, action);

    public Task<FactorData?> PopRedoAsync(string lectureId, int pageIndex)
        => _redis.PopRedoAsync(lectureId, pageIndex);

    public Task ClearRedoAsync(string lectureId, int pageIndex)
        => _redis.ClearRedoAsync(lectureId, pageIndex);

    public Task ClearHistoryAsync(string lectureId)
        => _redis.ClearHistoryAsync(lectureId);
}
