using SkiaSharp;

namespace canvasync.Library.Models;

public class Page
{
    public int PageIndex { get; set; } = 0;
    public string ImgUrl { get; set; } = string.Empty;
    public SKImage? Image { get; set; }
    public List<Factor> Factors { get; set; } = new();
    public List<Factor> HostFactors { get; set; } = new();
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
}