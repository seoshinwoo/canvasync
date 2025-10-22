using System.Drawing;
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

    public FactorPen(Factor factor)
    {
        if (factor.Paint is not null)
        {
            PenPath = new SKPath();
            Paint = factor.Paint;
            FactorType = FactorType.Pen;
        }
    }

    
    public override void Draw(SKCanvas canvas)
    {
        if (PenPath is not null)
        {
            canvas.DrawPath(PenPath, Paint);
        }
    }
}