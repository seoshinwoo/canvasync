using canvasync.Library.Dtos;

namespace canvasync.Services;

public interface IDrawingStorageService
{
    Task<bool> ContainsKeyAsync(string lectureId);
    Task<List<List<FactorDto>>?> GetAsync(string lectureId);
    Task SetAsync(string lectureId, List<List<FactorDto>> drawings);
    Task RemoveAsync(string lectureId);
}
