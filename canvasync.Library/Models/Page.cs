using SkiaSharp;

namespace Models;

public class Page
{
    public string PDFId { get; set; } = string.Empty;
    public int PageIndex { get; set; } = 0;
    public string ImgData { get; set; } = string.Empty;
    public SKImage? Image { get; set; }
    public List<Factor> Factors { get; set; } = new();
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
}