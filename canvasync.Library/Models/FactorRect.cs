using System.Net.Mail;
using Microsoft.VisualBasic;
using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorRect : Factor
{
    private SKFont font = new SKFont() { Size = 10 };
    public FactorRect()
    {
        
    }
    public FactorRect(Factor factor)
    {
        FactorType = factor.FactorType;

        Box = factor.Box;
        Paint = factor.Paint;
    }
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        var drawPaint = Paint.Clone();
        drawPaint.StrokeWidth = Paint.StrokeWidth * ratio;

        // canvas.DrawText($"({Box.Left},{Box.Top})", Box.Left * ratio + 10, Box.Top * ratio + 10, font, Paint);
        canvas.DrawRect(Box.Left * ratio + x, Box.Top * ratio + y, Box.Width * ratio, Box.Height * ratio, drawPaint);
    }
}