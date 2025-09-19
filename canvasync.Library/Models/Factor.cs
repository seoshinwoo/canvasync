using SkiaSharp;

namespace Models;

public class Factor
{
    // public FactorType FactorType = FactorType.None;
    public string FactorType = string.Empty;
    public SKRect Box { get; set; }
    public SKFont? Font { get; set; }
    public SKPaint? Paint { get; set; }
    public bool isSelected = false;
    public float TextWidth { get; set; }
    public float TextHeight { get; set; }
    public SKPaint _boxPaint = new SKPaint
    {
        Style = SKPaintStyle.Stroke,
        Color = SKColors.Red,
        StrokeWidth = 2,
        PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0)
    };

    public SKPaint _pointPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        StrokeWidth = 5,
        Color = SKColors.Orange
    };

    public Dictionary<string, SKPoint> PositionPoints => new()
    {
        {"LeftTop", new SKPoint(Box.Left, Box.Top)},
        {"MidTop", new SKPoint(Box.MidX, Box.Top)},
        {"RightTop", new SKPoint(Box.Right, Box.Top)},
        {"LeftMid", new SKPoint(Box.Left, Box.MidY)},
        {"RightMid", new SKPoint(Box.Right, Box.MidY)},
        {"LeftBottom", new SKPoint(Box.Left, Box.Bottom)},
        {"MidBottom", new SKPoint(Box.MidX, Box.Bottom)},
        {"RightBottom", new SKPoint(Box.Right, Box.Bottom)},
    };

    public virtual void Draw(SKCanvas canvas)
    {
        if (isSelected)
            canvas.DrawRect(Box, _boxPaint);
    }
}

public enum FactorType
{
    Rect,
    Circle,
    Line,
    Text,
    Pen,
    None
}