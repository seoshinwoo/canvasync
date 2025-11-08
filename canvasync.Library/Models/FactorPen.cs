using System.Drawing;
using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorPen : Factor
{
    public SKPath PenPath { get; set; }
    public FactorPen(SKPaint paint)
    {
        FactorType = FactorType.Pen;
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

    
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        if (PenPath is not null)
        {
            SKMatrix transformMatrix = SKMatrix.CreateIdentity();

            transformMatrix.ScaleX = ratio;
            transformMatrix.ScaleY = ratio;
            transformMatrix.TransX = x;
            transformMatrix.TransY = y;

            var path = new SKPath();

            PenPath.Transform(transformMatrix, path);

            var drawPaint = Paint.Clone();
            drawPaint.StrokeWidth = Paint.StrokeWidth * ratio;
            canvas.DrawPath(path, drawPaint);
        }
    }
}