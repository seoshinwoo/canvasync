
using System.Text.Json.Serialization;
using SkiaSharp;

namespace canvasync.Library.Models;

public class Factor
{
    private SKRect _box;
    private SKFont _font;
    private SKPaint _paint;
    public FactorType FactorType = FactorType.None;
    [JsonIgnore]
    public SKRect Box
    {
        get
        {
            if (_box.IsEmpty && FactorType is not FactorType.Pen)
            {
                _box = SKRect.Create(JsonBoxLeft, JsonBoxTop, JsonBoxWidth, JsonBoxHeight);
            }
            return _box;
        }
        set
        {
            _box = value;
            JsonBoxLeft = value.Left;
            JsonBoxTop = value.Top;
            JsonBoxWidth = value.Width;
            JsonBoxHeight = value.Height;
        }
    }
    [JsonIgnore]
    public SKFont Font
    {
        get
        {
            if (_font is null)
            {
                SKTypeface typeface = SKTypeface.FromFamilyName(JsonFontFamily, JsonFontWeight, (int)SKFontStyleWidth.Normal, JsonFontSlant);
                _font = new SKFont(typeface, JsonFontSize);
            }
            return _font;
        }
        set
        {
            _font = value;
            if (value is not null)
            {
                JsonFontFamily = value.Typeface?.FamilyName;
                JsonFontSize = value.Size;
                JsonFontWeight = value.Typeface?.FontWeight ?? (int)SKFontStyleWeight.Normal;
                JsonFontSlant = value.Typeface?.FontSlant ?? SKFontStyleSlant.Upright;
            }
        }
    }
    [JsonIgnore]
    public SKPaint Paint
    {
        get
        {
            if (_paint is null)
            {
                _paint = new SKPaint
                {
                    Style = JsonPaintStyle,
                    StrokeWidth = JsonPaintStrokeWidth,
                    IsAntialias = JsonPaintIsAntialias
                };
                if (SKColor.TryParse(JsonPaintColor, out SKColor color))
                {
                    _paint.Color = color;
                }
            }
            return _paint;
        }
        set
        {
            _paint = value;

            if (value != null)
            {
                JsonPaintColor = value.Color.ToString();
                JsonPaintStyle = value.Style;
                JsonPaintStrokeWidth = value.StrokeWidth;
                JsonPaintIsAntialias = value.IsAntialias;
            }
        }
    }
    // public float TextWidth { get; set; }
    // public float TextHeight { get; set; }

    // Box 직렬화를 위해..
    public float JsonBoxLeft { get; set; }
    public float JsonBoxTop { get; set; }
    public float JsonBoxWidth { get; set; }
    public float JsonBoxHeight { get; set; }

    // Font 직렬화를 위해..
    public string? JsonFontFamily { get; set; }
    public float JsonFontSize { get; set; }
    public int JsonFontWeight { get; set; }
    public SKFontStyleSlant JsonFontSlant { get; set; }

    // Paint 직렬화를 위해..
    public string? JsonPaintColor { get; set; }
    public SKPaintStyle JsonPaintStyle { get; set; }
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