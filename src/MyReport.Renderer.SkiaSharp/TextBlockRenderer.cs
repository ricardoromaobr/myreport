using System.Drawing;
using MyReport.Model;
using MyReport.Model.Controls;
using SkiaSharp;
using Size = MyReport.Model.Size;

namespace MyReport.Renderer.SkiaSharp;

public class TextBlockRenderer : IControlRenderer
{
    public void Render(object context, Control control)
    {
        SKCanvas c = context as SKCanvas;
        TextBlock textBlock = control as TextBlock;
        SKRect borderRect;

        c.Save();

        borderRect = new SKRect((float) textBlock.Location.X,
            (float) textBlock.Location.Y,
            (float) textBlock.Width,
            (float) textBlock.Height);

        if (!textBlock.CanGrow || DesignMode)
            c.ClipRect(borderRect);


        SKPaint paint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = new SKColor((byte) textBlock.BackgroundColor.R,
                (byte) textBlock.BackgroundColor.G,
                (byte) textBlock.BackgroundColor.B,
                (byte) textBlock.BackgroundColor.A),
        };

        var size = Measure(context, control);

        if (!DesignMode && (textBlock.CanGrow && size.Height > textBlock.Height ||
                            textBlock.CanShrink && size.Height < textBlock.Height))
        {
            borderRect = new SKRect((float) textBlock.Location.X, (float) textBlock.Location.Y, (float) textBlock.Width,
                (float) size.Height);
        }
        else
        {
            borderRect = new SKRect((float) textBlock.Location.X, (float) textBlock.Location.Y, (float) textBlock.Width,
                (float) textBlock.Height);
        }

        c.DrawRect(borderRect, paint);
        DrawBorder(textBlock, c, size);

        paint = CreatePaint(textBlock);

        WrapLines(textBlock, c, paint);

        c.Restore();
    }

    private void DrawBorder(TextBlock textBlock, SKCanvas c, Size size)
    {
        SKPaint paintBorder = new SKPaint
        {
            Color = new SKColor((byte) textBlock.Border.Color.R,
                (byte) textBlock.Border.Color.G,
                (byte) textBlock.Border.Color.B,
                (byte) textBlock.Border.Color.A),
            Style = SKPaintStyle.Stroke
        };


        //horizontal lines
        // top line
        var x1 = 0.0f;
        var y1 = 0.0f;
        var x2 = x1;
        var y2 = y1;

        if (textBlock.Border.TopWidth > 0)
        {
            paintBorder.StrokeWidth = (float) textBlock.Border.TopWidth;
            x1 = (float) textBlock.Location.X;
            x2 = (float) (textBlock.Location.X + textBlock.Width);
            y1 = (float) textBlock.Location.Y;
            y2 = y1;
            c.DrawLine(x1, y1, x2, y2, paintBorder);
        }

        // bottom line
        if (textBlock.Border.BottomWidth > 0)
        {
            y1 = (float) (y1 + size.Height - textBlock.Border.BottomWidth);
            y2 = y1;
            paintBorder.StrokeWidth = (float) textBlock.Border.BottomWidth;
            c.DrawLine(x1, y1, x2, y2, paintBorder);
        }

        //vertical lines
        //Left line 
        if (textBlock.Border.LeftWidth > 0)
        {
            paintBorder.StrokeWidth = (float) textBlock.Border.LeftWidth;
            x1 = (float) textBlock.Location.X;
            y1 = (float) textBlock.Location.Y;
            x2 = x1;
            y2 = (float) (y1 + size.Height);
            c.DrawLine(x1, y1, x2, y2, paintBorder);
        }

        // right line 
        if (textBlock.Border.RightWidth > 0)
        {
            paintBorder.StrokeWidth = (float) textBlock.Border.LeftWidth;
            x1 = (float) (textBlock.Location.X + size.Width - textBlock.Border.RightWidth);
            y1 = (float) (textBlock.Location.Y + size.Height);
            x2 = x1;
            y2 = (float) (y1 + size.Height);
            c.DrawLine(x1, y1, x2, y2, paintBorder);
        }
    }

    public Size Measure(object context, Control control)
    {
        SKCanvas c = context as SKCanvas;
        TextBlock textBlock = control as TextBlock;
        SKRect borderRect;

        var paint = CreatePaint(textBlock);

        var lines = WrappedLines(textBlock.Text, (float) textBlock.Width, paint);
        var size = CalcSize(lines, (float) textBlock.Width, paint);

        size.Height += textBlock.Padding.Top + textBlock.Padding.Bottom;
        size.Width += textBlock.Padding.Left + textBlock.Padding.Right;

        size.Height += textBlock.Border.TopWidth + textBlock.Border.BottomWidth;
        size.Width += textBlock.Border.LeftWidth + textBlock.Border.RightWidth;

        return size;
    }

    private SKPaint CreatePaint(TextBlock? textBlock)
    {
        SKFontStyleSlant fontStyleSlant = DefineFontStyleSlant(textBlock!.FontSlant);
        SKFontStyleWeight fontStyleWeight = DefineFontStyelWeight(textBlock.FontWeight);
        SKFontStyle fontStyle = new SKFontStyle(fontStyleWeight, SKFontStyleWidth.Normal, fontStyleSlant);
        SKTypeface typeface = SKTypeface.FromFamilyName(textBlock.FontName, fontStyle);

        SKPaint paint = new SKPaint
        {
            TextSize = (float) textBlock.FontSize,
            Typeface = typeface,
        };
        return paint;
    }

    private SKFontStyleWeight DefineFontStyelWeight(FontWeight textBlockFontWeight)
    {
        return textBlockFontWeight switch
        {
            FontWeight.Bold => SKFontStyleWeight.Bold,
            FontWeight.Normal => SKFontStyleWeight.Normal,
            _ => SKFontStyleWeight.Normal
        };
    }

    private SKFontStyleSlant DefineFontStyleSlant(FontSlant textBlockFontSlant)
    {
        return textBlockFontSlant switch
        {
            FontSlant.Normal => SKFontStyleSlant.Upright,
            FontSlant.Italic => SKFontStyleSlant.Italic,
            FontSlant.Oblique => SKFontStyleSlant.Oblique,
            _ => SKFontStyleSlant.Upright
        };
    }

    public double Dpi { get; set; }
    public bool DesignMode { get; set; }

    public Control[] BreakOffControlAtMostAtHeight(object context, Control control, double height)
    {
        Control[] controls = new Control[2];

        TextBlock textBlock = control.CreateControl() as TextBlock;


        return controls;
    }


    void WrapLines(TextBlock textBlock, SKCanvas canvas, SKPaint defPaint)
    {
        var wrappedLines = WrappedLines(textBlock.Text, (float) textBlock.Width, defPaint);
        var x = (float) (textBlock.Location.X + textBlock.Border.LeftWidth + textBlock.Padding.Left);
        var y = (float) (textBlock.Location.Y + textBlock.Border.TopWidth + textBlock.Padding.Top);

        SKRect rect = new SKRect();
        defPaint.MeasureText(wrappedLines[0], ref rect);

        x -= rect.Left;
        y -= rect.Top;

        foreach (var wrappedLine in wrappedLines)
        {
            canvas.DrawText(wrappedLine, x, y, defPaint);
            y += defPaint.FontSpacing;
        }
    }

    Size CalcSize(IList<string> wrapLines, float lineLengthLimit, SKPaint paint)
    {
        // define the surface properties
        var info = new SKImageInfo(256, 256 );

// construct a new surface
        var surface = SKSurface.Create(info);

// get the canvas from the surface
        var canvas = surface.Canvas;

// draw on the canvas ...

        var y = 0.0f;

        foreach (var wrapLine in wrapLines)
        {
            canvas.DrawText(wrapLine, 0,0, paint);
            y += paint.FontSpacing;
        }

        return new Size
        {
            Width = lineLengthLimit,
            Height = y
        };
    }

    private List<string> WrappedLines(string longLine, float lineLengthLimit, SKPaint defPaint)
    {
        var wrappedLines = new List<string>();
        var lineLength = 0f;
        var line = "";
        var words = longLine.Split(' ');
        if (words.Length == 1)
        {
            line = words[0];
        }
        else
        {
            var i = 0;
            var wordWithSpace = string.Empty; 
            
            foreach (var word in words)
            {
                if (i < words.Length - 1)
                    wordWithSpace = word + " ";
                else
                    wordWithSpace = word;
                
                var wordWithSpaceLength = defPaint.MeasureText(wordWithSpace);
                if (lineLength + wordWithSpaceLength > lineLengthLimit)
                {
                    wrappedLines.Add(line);
                    line = "" + wordWithSpace;
                    lineLength = wordWithSpaceLength;
                }
                else
                {
                    line += wordWithSpace;
                    lineLength += wordWithSpaceLength;
                }

                i++;
            }
        }

        wrappedLines.Add(line);
        return wrappedLines;
    }
}