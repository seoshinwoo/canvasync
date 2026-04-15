using canvasync.Library.Models;
using SkiaSharp;

namespace canvasync.Library.Dtos;

public class LectureDto
{
    public string Id { get; set; }
    public string Code { get; set; }
    public string FileName { get; set; }
    public string? PdfFileAddress { get; set; }
}