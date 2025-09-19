using SkiaSharp;

namespace Models;

public class FactorRect : Factor
{
    
    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawRect(Box, Paint);
    }
}