using canvasync.Library.Models;
using SkiaSharp;

namespace canvasync.Library.Dtos;

public class FactorDto
{
    public FactorType FactorType { get; set; } = FactorType.None;

    // Box 직렬화/역직렬화를 위해..
    public float BoxLeft { get; set; }
    public float BoxTop { get; set; }
    public float BoxWidth { get; set; }
    public float BoxHeight { get; set; }

    // Paint 직렬화/역직렬화를 위해..
    public string? PaintColor { get; set; }
    public SKPaintStyle PaintStyle { get; set; }
    public float PaintStrokeWidth { get; set; }
    public bool PaintIsAntialias { get; set; }

    // FactorText 직렬화/역직렬화를 위해..
    public List<TextBlockDto> TextBlockDtos { get; set; } = new();

    // FactorPen 직렬화/역직렬화를 위해..
    public PenPathDto PenPathDto { get; set; } = new();


    public static FactorDto FactorToFactorDto(Factor factor)
    {
        var factorDto = new FactorDto();
        factorDto.FactorType = factor.FactorType;

        // Box 직렬화..
        if (factor.FactorType is not FactorType.Pen)
        {
            factorDto.BoxLeft = factor.Box.Left;
            factorDto.BoxTop = factor.Box.Top;
            factorDto.BoxWidth = factor.Box.Width;
            factorDto.BoxHeight = factor.Box.Height;
        }

        // Paint 직렬화..
        factorDto.PaintColor = factor.Paint.Color.ToString();
        factorDto.PaintStyle = factor.Paint.Style;
        factorDto.PaintStrokeWidth = factor.Paint.StrokeWidth;
        factorDto.PaintIsAntialias = factor.Paint.IsAntialias;

        // TextBox 직렬화..
        if (factor.FactorType is FactorType.Text)
        {
            FactorText factorText = (FactorText)factor;
            factorDto.TextBlockDtos = new List<TextBlockDto>();

            foreach (var textBlock in factorText.TextBlocks)
            {
                var textBlockDto = new TextBlockDto();

                textBlockDto.TextBlockText = textBlock.Text;

                textBlockDto.TextBlockPaintStyle = textBlock.Paint.Style;
                textBlockDto.TextBlockPaintStrokeWidth = textBlock.Paint.StrokeWidth;
                textBlockDto.TextBlockPaintColor = textBlock.Paint.Color.ToString();
                textBlockDto.TextBlockPaintIsAntialias = textBlock.Paint.IsAntialias;

                textBlockDto.TextBlockFontFamily = textBlock.Font.Typeface?.FamilyName;
                textBlockDto.TextBlockFontSize = textBlock.Font.Size;
                textBlockDto.TextBlockFontWeight = textBlock.Font.Typeface?.FontWeight ?? (int)SKFontStyleWeight.Normal;
                textBlockDto.TextBlockFontSlant = textBlock.Font.Typeface?.FontSlant ?? SKFontStyleSlant.Upright;

                textBlockDto.TextBlockLeft = textBlock.Left;
                textBlockDto.TextBlockTop = textBlock.Top;
                textBlockDto.TextBlockWidth = textBlock.Width;
                textBlockDto.TextBlockHeight = textBlock.Height;

                factorDto.TextBlockDtos.Add(textBlockDto);
            }
        }

        if (factor is FactorPen)
        {
            Console.WriteLine($"FactorPen!!!!!!!!!");
        }

        // TextPen 직렬화..
        if (factor.FactorType is FactorType.Pen)
        {
            // var factorPen = new FactorPen(factor);
            FactorPen factorPen = (FactorPen)factor;

            var penPathDto = new PenPathDto();
            penPathDto.PenPathData = factorPen.PenPath.ToSvgPathData();
            factorDto.PenPathDto = penPathDto;
            Console.WriteLine($"factorDto.PenPathDto.PenPathData : {factorDto.PenPathDto.PenPathData.Length}");

            // FactorPen factorPen = (FactorPen)factor;
            // var penPathsDto = new PenPathsDto();
            // using var iterator = factorPen.PenPath.CreateRawIterator();

            // var points = new SKPoint[4];
            // SKPathVerb verb;

            // while ((verb = iterator.Next(points)) != SKPathVerb.Done)
            // {
            //     var penPathDto = new PenPathDto
            //     {
            //         Verb = verb.ToString() // "Move", "Quad" 등의 문자열로 저장
            //     };

            //     int pointCount = verb switch
            //     {
            //         SKPathVerb.Move => 1,
            //         SKPathVerb.Line => 2,
            //         SKPathVerb.Quad => 3,
            //         SKPathVerb.Conic => 3,
            //         SKPathVerb.Cubic => 4,
            //         SKPathVerb.Close => 0,
            //         _ => 0
            //     };

            //     penPathDto.Points.AddRange(points.Take(pointCount).Select(p => (p.X, p.Y)));
            //     penPathsDto.PenPaths.Add(penPathDto);
            // }

            // factorDto.PenPathDtos = penPathsDto.PenPaths;
        }

        return factorDto;
    }

    public static Factor FactorDtoToFactor(FactorDto factorDto)
    {
        var factor = new Factor();

        // Box 역직렬화..
        if (factorDto.FactorType is not FactorType.Pen)
        {
            factor.Box = new SKRect(factorDto.BoxLeft, factorDto.BoxTop, factorDto.BoxLeft + factorDto.BoxWidth, factorDto.BoxTop + factorDto.BoxHeight);
        }

        // Paint 역직렬화..
        factor.Paint = new SKPaint
        {
            Style = factorDto.PaintStyle,
            StrokeWidth = factorDto.PaintStrokeWidth,
            IsAntialias = factorDto.PaintIsAntialias
        };

        if (SKColor.TryParse(factorDto.PaintColor, out SKColor color))
        {
            factor.Paint.Color = color;
        }

        // FactorText 역직렬화..
        if (factorDto.FactorType is FactorType.Text)
        {
            var factorText = new FactorText(factor);

            foreach (var textBlockDto in factorDto.TextBlockDtos ?? new List<TextBlockDto>())
            {
                var textBlock = new TextBlock();

                textBlock.Text = textBlockDto.TextBlockText;

                textBlock.Paint.Style = textBlockDto.TextBlockPaintStyle;
                textBlock.Paint.StrokeWidth = textBlockDto.TextBlockPaintStrokeWidth;
                textBlock.Paint.IsAntialias = textBlockDto.TextBlockPaintIsAntialias;
                if (SKColor.TryParse(textBlockDto.TextBlockPaintColor, out SKColor textBlockColor))
                {
                    textBlock.Paint.Color = textBlockColor;
                }

                SKTypeface typeface = !string.IsNullOrEmpty(textBlockDto.TextBlockFontFamily)
                    ? SKTypeface.FromFamilyName(textBlockDto.TextBlockFontFamily, (int)textBlockDto.TextBlockFontWeight, (int)SKFontStyleWidth.Normal, textBlockDto.TextBlockFontSlant)
                    : SKTypeface.Default;
                textBlock.Font.Size = textBlockDto.TextBlockFontSize;

                textBlock.Left = textBlockDto.TextBlockLeft;
                textBlock.Top = textBlockDto.TextBlockTop;
                textBlock.Width = textBlockDto.TextBlockWidth;
                textBlock.Height = textBlockDto.TextBlockHeight;

                factorText.TextBlocks.Add(textBlock);
            }

            return factorText;
        }

        // FactorPen 역직렬화..
        if (factorDto.FactorType is FactorType.Pen)
        {
            var factorPen = new FactorPen(factor);
            Console.WriteLine($"factorDto.PenPathDto.PenPathData : {factorDto.PenPathDto.PenPathData.Length}");
            factorPen.PenPath = SKPath.ParseSvgPathData(factorDto.PenPathDto.PenPathData);



            // var path = new SKPath();

            // Console.WriteLine($"factorDto.PenPathDtos : {factorDto.PenPathDtos.Count()}");
            // foreach (var penPath in factorDto.PenPathDtos ?? new List<PenPathDto>())
            // {
            //     switch (penPath.Verb)
            //     {
            //         case "Move":
            //             if (penPath.Points.Any())
            //             {
            //                 path.MoveTo(penPath.Points[0].X, penPath.Points[0].Y);
            //             }
            //             break;
            //         case "Line":
            //             if (penPath.Points.Count >= 2)
            //             {
            //                 path.LineTo(penPath.Points[1].X, penPath.Points[1].Y);
            //             }
            //             break;
            //         case "Quad":
            //             if (penPath.Points.Count >= 3)
            //             {
            //                 path.QuadTo(penPath.Points[1].X, penPath.Points[1].Y, penPath.Points[2].X, penPath.Points[2].Y);
            //             }
            //             break;
            //         case "Cubic":
            //             if (penPath.Points.Count >= 4)
            //             {
            //                 path.CubicTo(penPath.Points[1].X, penPath.Points[1].Y, penPath.Points[2].X, penPath.Points[2].Y, penPath.Points[3].X, penPath.Points[3].Y);
            //             }
            //             break;
            //         case "Close":
            //             path.Close();
            //             break;
            //     }
            // }

            // factorPen.PenPath = path;
            Console.WriteLine($"factorPen.PenPath : {factorPen.PenPath.PointCount}");
            return factorPen; 
        }


        return factor;
    }
}


public class TextBlockDto
{
    public string TextBlockText { get; set; } = string.Empty;
    public SKPaintStyle TextBlockPaintStyle { get; set; }
    public float TextBlockPaintStrokeWidth { get; set; }
    public string? TextBlockPaintColor { get; set; }
    public bool TextBlockPaintIsAntialias { get; set; }
    public string? TextBlockFontFamily { get; set; }
    public float TextBlockFontSize { get; set; }
    public float TextBlockFontWeight { get; set; }
    public SKFontStyleSlant TextBlockFontSlant { get; set; }
    public float TextBlockLeft { get; set; }
    public float TextBlockTop { get; set; }
    public float TextBlockWidth { get; set; }
    public float TextBlockHeight { get; set; }
}

public class PenPathsDto
{
    public List<PenPathDto> PenPathDtos { get; set; } = new();
}

public class PenPathDto
{
    public string PenPathData { get; set; } = string.Empty;
    // public string? PenPathPaintColor { get; set; }
    // public SKPaintStyle PenPathPaintStyle { get; set; }
    // public float PenPathPaintStrokeWidth { get; set; }
    // public bool PenPathPaintIsAntialias { get; set; }
}