using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorText : Factor
{
    public List<TextBlock> TextBlocks { get; set; } = new(){new TextBlock(){ Text = string.Empty }};
    public string _text = string.Empty;
    public string Text
    {
        get
        {
            return _text;
        }

        set
        {
            // _text = value;
            TextBlocks[0].Text = value;
        }
    }
    private SKPaint _positionPaint = new SKPaint
    {
        Style = SKPaintStyle.Fill,
        StrokeWidth = 5,
        Color = SKColors.Orange
    };


    public FactorText(string color)
    {
        TextBlocks[0].Text = "text..";
        if (!string.IsNullOrEmpty(color))
        {
            if (SKColor.TryParse(color, out SKColor skColor))
            {
                TextBlocks[0].Paint = new SKPaint() { Color = skColor };
            }
        }
        TextBlocks[0].Font = new SKFont() { Size = 30 };

        TextBlocks[0].MeasureTextSize();
        Box = new SKRect(Box.Left, Box.Top, Box.Left + TextBlocks[0].Width, Box.Top + TextBlocks[0].Height);
    }

    public FactorText(Factor factor)
    {
        TextBlocks[0].Text = "text..";

        if (factor.Paint is not null)
        {
            TextBlocks[0].Paint = factor.Paint;
        }

        TextBlocks[0].Font = new SKFont() { Size = 30 };

        TextBlocks[0].MeasureTextSize();
        Box = factor.Box;
    }

    public void MeasureTextSize()
    {
        TextBlocks[0].Width = Font.MeasureText(TextBlocks[0].Text);
        Font.GetFontMetrics(out SKFontMetrics metrics);
        TextBlocks[0].Height = metrics.Descent - metrics.Ascent;

        if (TextBlocks[0].Width > Box.Width)
        {
            Box = new SKRect(Box.Left, Box.Top, Box.Left + TextBlocks[0].Width, Box.Top + TextBlocks[0].Height);
        }
    }

    public void TextChanged(string text)
    {
        // Text = text;
        TextBlocks[0].Text = text;
        MeasureTextSize();
    }
    public override void Draw(SKCanvas canvas)
    {
        foreach (var textBlock in TextBlocks)
        {
            canvas.DrawText(textBlock.Text, Box.Left, Box.Top + TextBlocks[0].Height, textBlock.Font, textBlock.Paint);
        }
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