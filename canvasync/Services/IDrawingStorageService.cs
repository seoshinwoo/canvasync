using canvasync.Library.Dtos;
using canvasync.Library.Models;

namespace canvasync.Services;

public interface IDrawingStorageService
{
    // ── 전체 드로잉 데이터 (STRING 기반) ──
    Task<bool> ContainsKeyAsync(string lectureId);
    Task<List<List<FactorDto>>?> GetAsync(string lectureId);
    Task SetAsync(string lectureId, List<List<FactorDto>> drawings);
    Task RemoveAsync(string lectureId);
    Task SetExpiryAsync(string lectureId, TimeSpan expiry);

    // Redis SET 기반 접속자 관리
    Task AddMemberAsync(string lectureId, string connectionId, bool isHost, string memberId);
    Task RemoveMemberAsync(string lectureId, string connectionId);
    Task<(bool IsHost, string MemberId)?> GetMemberInfoAsync(string lectureId, string connectionId);
    Task<long> GetMemberCountAsync(string lectureId);

    // connectionId → lectureId 역방향 매핑
    Task SetConnectionMappingAsync(string connectionId, string lectureId);
    Task<string?> GetLectureIdByConnectionAsync(string connectionId);
    Task RemoveConnectionMappingAsync(string connectionId);

}
