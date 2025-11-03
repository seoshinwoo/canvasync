using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorLine : Factor
{
    public FactorLine()
    {
        
    }
    public FactorLine(Factor factor)
    {
        FactorType = factor.FactorType;

        Box = factor.Box;
        Paint = factor.Paint;
    }
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        var drawPaint = Paint.Clone();
        drawPaint.StrokeWidth = Paint.StrokeWidth * ratio;

        canvas.DrawLine(Box.Left * ratio + x, Box.MidY * ratio + y, Box.Right * ratio + x, Box.MidY * ratio + y, drawPaint);
    }
}