using canvasync.Library.Dtos;

namespace canvasync.Library.Models;

public class FactorData
{
    public string LectureId { get; set; } = string.Empty;
    public int PageIndex { get; set; }
    public int FactorIndex { get; set; }
    public string? FactorAction { get; set; }
    public FactorDto FactorDto { get; set; } = new();
}