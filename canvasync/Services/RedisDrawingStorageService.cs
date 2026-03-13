using System.Text.Json;
using canvasync.Library.Dtos;
using StackExchange.Redis;

namespace canvasync.Services;

public class RedisDrawingStorageService : IDrawingStorageService
{
    private readonly IConnectionMultiplexer _multiplexer;
    private const string KeyPrefix = "drawing:";

    public RedisDrawingStorageService(IConnectionMultiplexer multiplexer)
    {
        _multiplexer = multiplexer;
    }

    private static string GetKey(string lectureId) => $"{KeyPrefix}{lectureId}";

    public async Task<bool> ContainsKeyAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        return await db.KeyExistsAsync(GetKey(lectureId));
    }

    public async Task<List<List<FactorDto>>?> GetAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        var value = await db.StringGetAsync(GetKey(lectureId));
        if (!value.HasValue) return null;
        return JsonSerializer.Deserialize<List<List<FactorDto>>>(value!);
    }

    public async Task SetAsync(string lectureId, List<List<FactorDto>> drawings)
    {
        var db = _multiplexer.GetDatabase();
        var json = JsonSerializer.Serialize(drawings);
        await db.StringSetAsync(GetKey(lectureId), json);
    }

    public async Task RemoveAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        await db.KeyDeleteAsync(GetKey(lectureId));
    }

    public async Task SetExpiryAsync(string lectureId, TimeSpan expiry)
    {
        var db = _multiplexer.GetDatabase();
        await db.KeyExpireAsync(GetKey(lectureId), expiry);
    }

    // ── Redis SET 기반 접속자 관리 ──
    // Key: "room:{lectureId}"  (SET)
    // Value: "{connectionId}:{isHost}:{memberId}"
    private static string GetRoomKey(string lectureId) => $"room:{lectureId}";

    private static string EncodeMember(string connectionId, bool isHost, string memberId)
        => $"{connectionId}:{(isHost ? 1 : 0)}:{memberId}";

    private static (string ConnectionId, bool IsHost, string MemberId) DecodeMember(string value)
    {
        var parts = value.Split(':', 3);
        return (parts[0], parts[1] == "1", parts[2]);
    }

    public async Task AddMemberAsync(string lectureId, string connectionId, bool isHost, string memberId)
    {
        var db = _multiplexer.GetDatabase();
        await db.SetAddAsync(GetRoomKey(lectureId), EncodeMember(connectionId, isHost, memberId));
    }

    public async Task RemoveMemberAsync(string lectureId, string connectionId)
    {
        var db = _multiplexer.GetDatabase();
        var members = await db.SetMembersAsync(GetRoomKey(lectureId));
        foreach (var member in members)
        {
            if (member.ToString().StartsWith($"{connectionId}:"))
            {
                await db.SetRemoveAsync(GetRoomKey(lectureId), member);
                break;
            }
        }
    }

    public async Task<(bool IsHost, string MemberId)?> GetMemberInfoAsync(string lectureId, string connectionId)
    {
        var db = _multiplexer.GetDatabase();
        var members = await db.SetMembersAsync(GetRoomKey(lectureId));
        foreach (var member in members)
        {
            var decoded = DecodeMember(member.ToString());
            if (decoded.ConnectionId == connectionId)
                return (decoded.IsHost, decoded.MemberId);
        }
        return null;
    }

    public async Task<long> GetMemberCountAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        return await db.SetLengthAsync(GetRoomKey(lectureId));
    }

    // ── connectionId → lectureId 역방향 매핑 ──
    private static string GetConnKey(string connectionId) => $"conn:{connectionId}";

    public async Task SetConnectionMappingAsync(string connectionId, string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        await db.StringSetAsync(GetConnKey(connectionId), lectureId, TimeSpan.FromHours(4));
    }

    public async Task<string?> GetLectureIdByConnectionAsync(string connectionId)
    {
        var db = _multiplexer.GetDatabase();
        var value = await db.StringGetAsync(GetConnKey(connectionId));
        return value.HasValue ? value.ToString() : null;
    }

    public async Task RemoveConnectionMappingAsync(string connectionId)
    {
        var db = _multiplexer.GetDatabase();
        await db.KeyDeleteAsync(GetConnKey(connectionId));
    }

}
