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
        // var defaultText = "text..";
        // var defaultFontSize = 30;
        // var textBlock = new TextBlock()
        // {
        //     Text = defaultText,
        //     Font = new SKFont() { Size = defaultFontSize },
        //     Paint = paint,
        //     Left = 0,
        //     Top = 0
        // };
        // textBlock.MeasureTextSize();

        // if (TextBlocks.Count == 0)
        // {
        //     Box = new SKRect(Box.Left, Box.Top, Box.Left + textBlock.Width, Box.Top + textBlock.Height);
        // }

        // TextBlocks.Add(textBlock);

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

    // public void MeasureText()
    // {
    //     if (Font is not null)
    //     {
    //         var textWidth = Font.MeasureText(Text);
    //         Font.GetFontMetrics(out SKFontMetrics metrics);
    //         Box = new SKRect(Box.Left, Box.Top, Box.Left + textWidth, Box.Top + (metrics.Descent - metrics.Ascent));
    //         _textYPosition = (metrics.Ascent + metrics.Descent) / 2;
    //     }
    // }
    public override void Draw(SKCanvas canvas)
    {
        if (isSelected)
        {
            canvas.DrawRect(Box, _boxPaint);

            canvas.DrawPoint(new SKPoint(Box.Left, Box.Top), _positionPaint);
            canvas.DrawPoint(new SKPoint(Box.MidX, Box.Top), _positionPaint);
            canvas.DrawPoint(new SKPoint(Box.Right, Box.Top), _positionPaint);
            canvas.DrawPoint(new SKPoint(Box.Left, Box.MidY), _positionPaint);
            canvas.DrawPoint(new SKPoint(Box.Right, Box.MidY), _positionPaint);
            canvas.DrawPoint(new SKPoint(Box.Left, Box.Bottom), _positionPaint);
            canvas.DrawPoint(new SKPoint(Box.MidX, Box.Bottom), _positionPaint);
            canvas.DrawPoint(new SKPoint(Box.Right, Box.Bottom), _positionPaint);
        }

        // foreach (var textBlock in TextBlocks)
        // {
        //     canvas.DrawText(textBlock.Text, Box.Left + textBlock.Left, Box.Top + textBlock.Height, textBlock.Font, textBlock.Paint);
        // }

        canvas.DrawText(Text, Box.Left, Box.Top + TextHeight, Font, Paint);
        Console.WriteLine($"");

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