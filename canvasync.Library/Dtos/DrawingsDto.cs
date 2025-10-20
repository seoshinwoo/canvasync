using canvasync.Library.Models;
using SkiaSharp;

namespace canvasync.Library.Dtos;

public class DrawingsDto
{
    public List<PageDto> PageDtos { get; set; } = new();
    public float PageWidth { get; set; }
    public float PageHeight { get; set; }
    public float FactorRatio { get; set; }
}