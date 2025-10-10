using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorRect : Factor
{
    
    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawRect(Box, Paint);
    }
}