using System.Text.Json.Serialization;
using SkiaSharp;

namespace Models;

public class Factor
{
    public FactorType FactorType = FactorType.None;
    [JsonIgnore]
    public SKRect Box { get; set; }
    [JsonIgnore]
    public SKFont? Font { get; set; }
    [JsonIgnore]
    public SKPaint? Paint { get; set; }
    public float TextWidth { get; set; }
    public float TextHeight { get; set; }

    // Box 직렬화를 위해..
    public float JsonBoxLeft { get; set; }
    public float JsonBoxTop { get; set; }
    public float JsonBoxWidth { get; set; }
    public float JsonBoxHeight { get; set; }

    // Font 직렬화를 위해..
    public string? JsonFontFamily { get; set; }
    public float JsonFontSize { get; set; }
    public float JsonFontWeight { get; set; }

    // Paint 직렬화를 위해..
    public string? JsonPaintColor { get; set; }
    public string? JsonPaintStyle { get; set; }
    public float JsonPaintStrokeWidth { get; set; }
    public bool JsonPaintIsAntialias { get; set; }

    // FactorText 직렬화를 위해..
    public List<TextBlockDto>? JsonTextBlockDtos { get; set; }

    // FactorPen 직렬화를 위해..
    public List<PenPathDto>? JsonPenPathDtos { get; set; }
    

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

public class TextBlockDto
{
    public string JsonTextBlockText { get; set; } = string.Empty;
    public string? JsonTextBlockPaintStyle { get; set; }
    public float JsonTextBlockPaintStrokeWidth { get; set; }
    public string? JsonTextBlockPaintColor { get; set; }
    public bool JsonTextBlockJsonPaintIsAntialias { get; set; }
    public string? JsonTextBlockFontFamily { get; set; }
    public float JsonTextBlockFontSize { get; set; }
    public float JsonTextBlockFontWeight { get; set; }
    public float JsonTextBlockLeft { get; set; }
    public float JsonTextBlockTop { get; set; }
}

public class PenPathDto
{
    // "Move", "Line", "Quad", "Cubic", "Close"
    public string Verb { get; set; } = string.Empty;
    public List<(float X, float Y)> Points { get; set; } = new();
}