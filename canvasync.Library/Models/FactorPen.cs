using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorPen : Factor
{
    public SKPath PenPath { get; set; }
    public FactorPen(SKPaint paint)
    {
        PenPath = new SKPath();
        Paint = paint;
    }
    public override void Draw(SKCanvas canvas)
    {
        if (PenPath is not null)
        {
            canvas.DrawPath(PenPath, Paint);
        }
    }
}