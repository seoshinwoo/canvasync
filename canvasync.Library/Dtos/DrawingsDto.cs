using canvasync.Library.Models;
using SkiaSharp;

namespace canvasync.Library.Dtos;

public class DrawingsDto
{
    public List<PageDto> PageDtos { get; set; } = new();
}