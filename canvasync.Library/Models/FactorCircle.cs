using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorCircle : Factor
{
    public FactorCircle()
    {
        
    }
    public FactorCircle(Factor factor)
    {
        FactorType = factor.FactorType;

        Box = factor.Box;
        Paint = factor.Paint;
    }
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        var drawPaint = Paint.Clone();
        drawPaint.StrokeWidth = Paint.StrokeWidth * ratio;

        var radius = Box.Width >= Box.Height ? Box.Height / 2 : Box.Width / 2;
        canvas.DrawCircle(Box.MidX * ratio + x, Box.MidY * ratio + y, radius * ratio, drawPaint);
    }
}