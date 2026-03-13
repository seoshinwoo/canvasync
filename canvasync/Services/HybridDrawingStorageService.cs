using System.Collections.Concurrent;
using canvasync.Library.Dtos;
using canvasync.Library.Models;

namespace canvasync.Services;

/// <summary>
/// L1(ConcurrentDictionary) + L2(Redis) 하이브리드 캐싱 서비스.
/// 드로잉 데이터는 인메모리 캐시를 우선 조회하여 Redis 왕복을 줄이고,
/// Redis는 영속성과 수평 확장(다중 서버 인스턴스 동기화)을 위한 L2로 사용합니다.
/// 접속자 관리와 연결 매핑은 Redis에 직접 위임합니다.
/// </summary>
public class HybridDrawingStorageService : IDrawingStorageService
{
    private readonly RedisDrawingStorageService _redis;

    // L1 캐시: lectureId → drawings
    private readonly ConcurrentDictionary<string, List<List<FactorDto>>> _cache = new();

    public HybridDrawingStorageService(RedisDrawingStorageService redis)
    {
        _redis = redis;
    }

    // ── 전체 드로잉 데이터 (STRING 기반) ──

    public async Task<bool> ContainsKeyAsync(string lectureId)
    {
        if (_cache.ContainsKey(lectureId)) return true;
        return await _redis.ContainsKeyAsync(lectureId);
    }

    public async Task<List<List<FactorDto>>?> GetAsync(string lectureId)
    {
        // L1 히트
        if (_cache.TryGetValue(lectureId, out var cached))
        {
            return cached;
        }

        // L1 미스 → L2 조회 후 L1에 캐시
        var fromRedis = await _redis.GetAsync(lectureId);
        if (fromRedis != null)
        {
            _cache[lectureId] = fromRedis;
        }
        return fromRedis;
    }

    public async Task SetAsync(string lectureId, List<List<FactorDto>> drawings)
    {
        // L1 업데이트
        _cache[lectureId] = drawings;

        // L2 write-through
        await _redis.SetAsync(lectureId, drawings);
    }

    public async Task RemoveAsync(string lectureId)
    {
        _cache.TryRemove(lectureId, out _);
        await _redis.RemoveAsync(lectureId);
    }

    public Task SetExpiryAsync(string lectureId, TimeSpan expiry)
    {
        // L1에는 TTL 개념이 없으므로 Redis만 설정.
        return _redis.SetExpiryAsync(lectureId, expiry);
    }

    // ── 접속자 관리, 연결 매핑, 히스토리 → Redis 직접 위임 ──

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

}
