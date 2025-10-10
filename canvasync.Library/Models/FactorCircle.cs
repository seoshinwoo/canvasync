using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorCircle : Factor
{
    public override void Draw(SKCanvas canvas)
    {
        var radius = Box.Width >= Box.Height ? Box.Height / 2 : Box.Width / 2;
        canvas.DrawCircle(Box.MidX, Box.MidY, radius, Paint);
    }
}