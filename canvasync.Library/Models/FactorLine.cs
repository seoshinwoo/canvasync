using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorLine : Factor
{
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        canvas.DrawLine(Box.Left * ratio + x, Box.MidY * ratio + y, Box.Right * ratio + x, Box.MidY * ratio + y, Paint);
    }
}