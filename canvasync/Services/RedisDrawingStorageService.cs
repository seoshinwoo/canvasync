using System.Text.Json;
using canvasync.Library.Dtos;
using canvasync.Library.Models;
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

    private static string GetHashKey(string lectureId) => $"{KeyPrefix}{lectureId}";
    private static string GetPageField(int pageIndex) => $"page:{pageIndex}";

    public async Task<bool> ContainsKeyAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        return await db.KeyExistsAsync(GetHashKey(lectureId));
    }

    public async Task<List<List<FactorDto>>?> GetAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        var entries = await db.HashGetAllAsync(GetHashKey(lectureId));
        if (entries.Length == 0) return null;

        // page:0, page:1, ... 순서로 정렬하여 리스트 구성
        var pages = new SortedDictionary<int, List<FactorDto>>();
        foreach (var entry in entries)
        {
            var field = entry.Name.ToString();
            if (!field.StartsWith("page:")) continue;
            var index = int.Parse(field[5..]);
            var factors = JsonSerializer.Deserialize<List<FactorDto>>(entry.Value!);
            pages[index] = factors ?? new List<FactorDto>();
        }

        if (pages.Count == 0) return null;

        var maxIndex = pages.Keys.Max();
        var result = new List<List<FactorDto>>();
        for (int i = 0; i <= maxIndex; i++)
        {
            result.Add(pages.TryGetValue(i, out var p) ? p : new List<FactorDto>());
        }
        return result;
    }

    public async Task SetAsync(string lectureId, List<List<FactorDto>> drawings)
    {
        var db = _multiplexer.GetDatabase();
        var key = GetHashKey(lectureId);

        // 기존 Hash 제거 후 새로 저장
        await db.KeyDeleteAsync(key);

        var entries = new HashEntry[drawings.Count];
        for (int i = 0; i < drawings.Count; i++)
        {
            entries[i] = new HashEntry(GetPageField(i), JsonSerializer.Serialize(drawings[i]));
        }
        await db.HashSetAsync(key, entries);
        await db.KeyExpireAsync(key, TimeSpan.FromHours(2));
    }

    public async Task RemoveAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        await db.KeyDeleteAsync(GetHashKey(lectureId));
    }

    public async Task SetExpiryAsync(string lectureId, TimeSpan expiry)
    {
        var db = _multiplexer.GetDatabase();
        await db.KeyExpireAsync(GetHashKey(lectureId), expiry);
    }

    // ── 페이지 단위 Hash 메서드 ──
    public async Task<List<FactorDto>?> GetPageAsync(string lectureId, int pageIndex)
    {
        var db = _multiplexer.GetDatabase();
        var data = await db.HashGetAsync(GetHashKey(lectureId), GetPageField(pageIndex));
        if (!data.HasValue) return null;
        return JsonSerializer.Deserialize<List<FactorDto>>(data!);
    }

    public async Task SetPageAsync(string lectureId, int pageIndex, List<FactorDto> factors)
    {
        var db = _multiplexer.GetDatabase();
        var json = JsonSerializer.Serialize(factors);
        await db.HashSetAsync(GetHashKey(lectureId), GetPageField(pageIndex), json);
    }

    public async Task<int> GetPageCountAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        return (int)await db.HashLengthAsync(GetHashKey(lectureId));
    }

    public async Task EnsurePageCountAsync(string lectureId, int requiredPageCount)
    {
        var db = _multiplexer.GetDatabase();
        var key = GetHashKey(lectureId);
        for (int i = 0; i < requiredPageCount; i++)
        {
            var field = GetPageField(i);
            if (!await db.HashExistsAsync(key, field))
            {
                await db.HashSetAsync(key, field, "[]");
            }
        }
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

    // ── Redis LIST 기반 드로잉 히스토리 (Undo/Redo) ──
    // Undo 스택: "history:{lectureId}:{pageIndex}"  (LIST)
    // Redo 스택: "redo:{lectureId}:{pageIndex}"     (LIST)
    private const int MaxHistorySize = 50;
    private static string GetHistoryKey(string lectureId, int pageIndex) => $"history:{lectureId}:{pageIndex}";
    private static string GetRedoKey(string lectureId, int pageIndex) => $"redo:{lectureId}:{pageIndex}";

    public async Task PushHistoryAsync(string lectureId, int pageIndex, FactorData action)
    {
        var db = _multiplexer.GetDatabase();
        var key = GetHistoryKey(lectureId, pageIndex);
        var json = JsonSerializer.Serialize(action);
        await db.ListLeftPushAsync(key, json);
        await db.ListTrimAsync(key, 0, MaxHistorySize - 1);
        await db.KeyExpireAsync(key, TimeSpan.FromHours(4));
    }

    public async Task<FactorData?> PopHistoryAsync(string lectureId, int pageIndex)
    {
        var db = _multiplexer.GetDatabase();
        var value = await db.ListLeftPopAsync(GetHistoryKey(lectureId, pageIndex));
        if (!value.HasValue) return null;
        return JsonSerializer.Deserialize<FactorData>(value!);
    }

    public async Task PushRedoAsync(string lectureId, int pageIndex, FactorData action)
    {
        var db = _multiplexer.GetDatabase();
        var key = GetRedoKey(lectureId, pageIndex);
        var json = JsonSerializer.Serialize(action);
        await db.ListLeftPushAsync(key, json);
        await db.ListTrimAsync(key, 0, MaxHistorySize - 1);
        await db.KeyExpireAsync(key, TimeSpan.FromHours(4));
    }

    public async Task<FactorData?> PopRedoAsync(string lectureId, int pageIndex)
    {
        var db = _multiplexer.GetDatabase();
        var value = await db.ListLeftPopAsync(GetRedoKey(lectureId, pageIndex));
        if (!value.HasValue) return null;
        return JsonSerializer.Deserialize<FactorData>(value!);
    }

    public async Task ClearRedoAsync(string lectureId, int pageIndex)
    {
        var db = _multiplexer.GetDatabase();
        await db.KeyDeleteAsync(GetRedoKey(lectureId, pageIndex));
    }

    public async Task ClearHistoryAsync(string lectureId)
    {
        var db = _multiplexer.GetDatabase();
        var server = _multiplexer.GetServers()[0];
        await foreach (var key in server.KeysAsync(pattern: $"history:{lectureId}:*"))
        {
            await db.KeyDeleteAsync(key);
        }
        await foreach (var key in server.KeysAsync(pattern: $"redo:{lectureId}:*"))
        {
            await db.KeyDeleteAsync(key);
        }
    }
}
