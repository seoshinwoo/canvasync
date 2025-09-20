using SkiaSharp;

namespace Models;

public class FactorText : Factor
{
    public List<TextBlock> TextBlocks { get; set; } = new() { };
    private float _textYPosition;
    public string _text = string.Empty;
    public string Text
    {
        get
        {
            return _text;
        }

        set
        {
            _text = value;
        }
    }
    private SKPaint _positionPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        StrokeWidth = 5,
        Color = SKColors.Orange
    };


    public FactorText(SKPaint paint)
    {
        Text = "text..";
        Font = new SKFont() { Size = 30 };
        Paint = paint;

        MeasureTextSize();
        Box = new SKRect(Box.Left, Box.Top, Box.Left + TextWidth, Box.Top + TextHeight);
    }

    public void MeasureTextSize()
    {
        TextWidth = Font.MeasureText(Text);
        Font.GetFontMetrics(out SKFontMetrics metrics);
        TextHeight = metrics.Descent - metrics.Ascent;

        if (TextWidth > Box.Width)
        {
            Box = new SKRect(Box.Left, Box.Top, Box.Left + TextWidth, Box.Top + Box.Height);
        }
    }

    public void TextChanged(string text)
    {
        Text = text;
        MeasureTextSize();
    }
    public override void Draw(SKCanvas canvas)
    {
        canvas.DrawText(Text, Box.Left, Box.Top + TextHeight, Font, Paint);
    }
}

public class TextBlock
{
    public string Text { get; set; } = string.Empty;
    public SKPaint Paint { get; set; } = new SKPaint();
    public SKFont Font { get; set; } = new SKFont();
    public float Top { get; set; }
    public float Left { get; set; }
    public float Height { get; set; }
    public float Width { get; set; }

    public void MeasureTextSize()
    {
        Width = Font.MeasureText(Text);
        Font.GetFontMetrics(out SKFontMetrics metrics);
        Height = metrics.Descent - metrics.Ascent;
    }
}