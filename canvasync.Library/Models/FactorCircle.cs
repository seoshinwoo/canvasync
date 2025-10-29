using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorCircle : Factor
{
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        var radius = Box.Width >= Box.Height ? Box.Height / 2 : Box.Width / 2;
        canvas.DrawCircle(Box.MidX * ratio + x, Box.MidY * ratio + y, radius * ratio, Paint);
    }


}