using canvasync.Library.Dtos;
using canvasync.Library.Models;

namespace canvasync.Services;

public interface IDrawingStorageService
{
    // ── 전체 페이지 (Hash 기반) ──
    Task<bool> ContainsKeyAsync(string lectureId);
    Task<List<List<FactorDto>>?> GetAsync(string lectureId);
    Task SetAsync(string lectureId, List<List<FactorDto>> drawings);
    Task RemoveAsync(string lectureId);
    Task SetExpiryAsync(string lectureId, TimeSpan expiry);

    // ── 페이지 단위 (Hash field) ──
    Task<List<FactorDto>?> GetPageAsync(string lectureId, int pageIndex);
    Task SetPageAsync(string lectureId, int pageIndex, List<FactorDto> factors);
    Task<int> GetPageCountAsync(string lectureId);
    Task EnsurePageCountAsync(string lectureId, int requiredPageCount);

    // Redis SET 기반 접속자 관리
    Task AddMemberAsync(string lectureId, string connectionId, bool isHost, string memberId);
    Task RemoveMemberAsync(string lectureId, string connectionId);
    Task<(bool IsHost, string MemberId)?> GetMemberInfoAsync(string lectureId, string connectionId);
    Task<long> GetMemberCountAsync(string lectureId);

    // connectionId → lectureId 역방향 매핑
    Task SetConnectionMappingAsync(string connectionId, string lectureId);
    Task<string?> GetLectureIdByConnectionAsync(string connectionId);
    Task RemoveConnectionMappingAsync(string connectionId);

    // ── Redis LIST 기반 드로잉 히스토리 (Undo/Redo) ──
    Task PushHistoryAsync(string lectureId, int pageIndex, FactorData action);
    Task<FactorData?> PopHistoryAsync(string lectureId, int pageIndex);
    Task PushRedoAsync(string lectureId, int pageIndex, FactorData action);
    Task<FactorData?> PopRedoAsync(string lectureId, int pageIndex);
    Task ClearRedoAsync(string lectureId, int pageIndex);
    Task ClearHistoryAsync(string lectureId);
}
