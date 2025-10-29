
using System.Text.Json.Serialization;
using SkiaSharp;

namespace canvasync.Library.Models;

public class Factor
{
    private SKRect _box;
    private SKFont _font;
    private SKPaint _paint;
    public FactorType FactorType = FactorType.None;
    public SKRect Box
    {
        get
        {
            return _box;
        }
        set
        {
            _box = value;
        }
    }
    public SKFont Font
    {
        get
        {
            return _font;
        }
        set
        {
            _font = value;
        }
    }
    public SKPaint Paint
    {
        get
        {
            return _paint;
        }
        set
        {
            _paint = value;
        }
    }
    

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


    public virtual void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {

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