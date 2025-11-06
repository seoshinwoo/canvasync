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
            return TextBlocks[0].Text;
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
        TextBlocks[0].Text = text;
        TextBlocks[0].MeasureTextSize();

        Box = new SKRect(Box.Left, Box.Top, Box.Left + TextBlocks[0].Width, Box.Bottom);
    }
    public override void Draw(SKCanvas canvas, float ratio = 1f, float x = 0, float y = 0)
    {
        foreach (var textBlock in TextBlocks)
        {
            // Console.WriteLine($"Box -> Left : {Box.Left}, Top : {Box.Top}, Width : {Box.Width}, Height : {Box.Height}");
            // Console.WriteLine($"텍스트 -> Left : {textBlock.Left}, Top : {textBlock.Top}, Width : {textBlock.Width}, Height : {textBlock.Height}");
            var drawFont = new SKFont();
            drawFont.Size = textBlock.Font.Size * ratio;

            var drawPaint = Paint.Clone();
            drawPaint.StrokeWidth = textBlock.Paint.StrokeWidth * ratio;

            canvas.DrawText(textBlock.Text, (Box.Left + textBlock.Left) * ratio + x, (Box.Top + textBlock.Height) * ratio + y, drawFont, drawPaint);
            // Console.WriteLine($"텍스트 그림!! : {textBlock.Text}");
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

        // SKRect textBounds = new SKRect();
        // Font.MeasureText(Text, out textBounds);
    }
}