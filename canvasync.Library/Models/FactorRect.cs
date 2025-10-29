using Microsoft.VisualBasic;
using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorRect : Factor
{
    private SKFont font = new SKFont() { Size = 10 };
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        // canvas.DrawText($"({Box.Left},{Box.Top})", Box.Left * ratio + 10, Box.Top * ratio + 10, font, Paint);
        canvas.DrawRect(Box.Left * ratio + x, Box.Top * ratio + y, Box.Width * ratio, Box.Height * ratio, Paint);
    }
}