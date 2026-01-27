using SkiaSharp;

namespace canvasync.Library.Models;

public class FactorText : Factor
{
    public static SKTypeface Typeface;
    public List<TextBlock> TextBlocks { get; set; } = new(){new TextBlock(){ Text = string.Empty }};
    public string _text = string.Empty;
    public string Text
    {
        get
        {
            return TextBlocks[0].Text;
        }

        set
        {
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
        TextBlocks[0].Font = new SKFont() { Size = 300 };

        // MeasureText() 에서 Font가 Null 이여서 에러나길래 일단 추가해줌..
        Font = new SKFont();

        TextBlocks[0].MeasureTextSize();
        Box = new SKRect(Box.Left, Box.Top, Box.Left + TextBlocks[0].Width, Box.Top + TextBlocks[0].Height);
    }

    public FactorText(Factor factor)
    {
        FactorType = FactorType.Text;

        TextBlocks[0].Text = "text..";

        if (factor.Paint is not null)
        {
            TextBlocks[0].Paint = factor.Paint;
        }

        TextBlocks[0].Font = new SKFont() { Size = 300 };

        TextBlocks[0].MeasureTextSize();
        Box = factor.Box;
    }

    public void TextChanged(string text)
    {
        TextBlocks[0].Text = text;
        TextBlocks[0].MeasureTextSize();

        Box = new SKRect(Box.Left, Box.Top, Box.Left + TextBlocks[0].Width, Box.Top + TextBlocks[0].Height);
    }

    public void TextSizeChanged()
    {
        TextBlocks[0].MeasureTextSize();
        
        Box = new SKRect(Box.Left, Box.Top, Box.Left + TextBlocks[0].Width, Box.Top + TextBlocks[0].Height);
    }
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        foreach (var textBlock in TextBlocks)
        {
            var drawFont = new SKFont(Typeface);
            drawFont.Size = textBlock.Font.Size * ratio;

            // var drawPaint = textBlock.Paint.Clone();
            // drawPaint.StrokeWidth = textBlock.Paint.StrokeWidth * ratio;

            canvas.DrawText(textBlock.Text, (Box.Left + textBlock.Left) * ratio + x, (Box.Top + textBlock.Height) * ratio + y, drawFont, textBlock.Paint);
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
        Font.Typeface = FactorText.Typeface;
        Width = Font.MeasureText(Text);
        Font.GetFontMetrics(out SKFontMetrics metrics);
        Height = metrics.Descent - metrics.Ascent;
    }
}