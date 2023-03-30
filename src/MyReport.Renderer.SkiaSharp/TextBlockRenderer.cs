using System.Drawing;
using System.Net.Mime;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using MyReport.Model;
using MyReport.Model.Controls;
using MyReport.Model.Data;
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
            (float) (textBlock.Location.X + textBlock.Width),
            (float) textBlock.Bottom);

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

        var size = new Size();

        if (!DesignMode && (textBlock.CanGrow && size.Height > textBlock.Height ||
                            textBlock.CanShrink && size.Height < textBlock.Height))
        {
            borderRect = new SKRect(
                (float) textBlock.Location.X, 
                (float) textBlock.Location.Y, 
                (float) textBlock.Width,
                (float) textBlock.Bottom);
        }
        else
        {
            borderRect = new SKRect((float) textBlock.Location.X, 
                (float) textBlock.Location.Y, 
                (float) (textBlock.Location.X + textBlock.Width),
                (float) textBlock.Bottom);
        }
        
        //c.ClipRect(borderRect);
        c.DrawRect(borderRect, paint);
        DrawBorder(textBlock, c, size);

        paint = CreatePaint(textBlock);

        WrapLinesOnRender(textBlock, c, paint);

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

        
        // draw the top line 
        if (textBlock.Border.TopWidth > 0)
        {
            paintBorder.StrokeWidth = (float) textBlock.Border.TopWidth;
            x1 = (float) textBlock.Location.X;
            x2 = (float) (textBlock.Location.X + textBlock.Width);
            y1 = (float) textBlock.Location.Y + 1;
            y2 = y1;
            c.DrawLine(x1, y1, x2, y2, paintBorder);
        }

        // draw the bottom line
        if (textBlock.Border.BottomWidth > 0)
        {
            y1 = (float) (textBlock.Bottom - textBlock.Border.BottomWidth)+1;
            y2 = y1;
            paintBorder.StrokeWidth = (float) textBlock.Border.BottomWidth;
            c.DrawLine(x1, y1, x2, y2, paintBorder);
        }

        //vertical lines
        //Left line 
        if (textBlock.Border.LeftWidth > 0)
        {
            paintBorder.StrokeWidth = (float) textBlock.Border.LeftWidth;
            x1 = (float) textBlock.Location.X+1;
            y1 = (float) textBlock.Location.Y;
            x2 = x1;
            y2 = (float) (y1 + textBlock.Height);
            c.DrawLine(x1, y1, x2, y2, paintBorder);
        }

        // right line 
        if (textBlock.Border.RightWidth > 0)
        {
            paintBorder.StrokeWidth = (float) textBlock.Border.LeftWidth;
            x1 = (float) (textBlock.Location.X + textBlock.Width - textBlock.Border.RightWidth)+1;
            y1 = (float) (textBlock.Location.Y);
            x2 = x1;
            y2 = (float) (y1 + textBlock.Height);
            c.DrawLine(x1, y1, x2, y2, paintBorder);
        }
    }

    public Size Measure(object context, Control control)
    {
        SKCanvas c = context as SKCanvas;
        TextBlock textBlock = control as TextBlock;
        SKRect borderRect;
        
        var paint = CreatePaint(textBlock);
        
        if (textBlock.FieldKind == FieldKind.Expression && textBlock.FieldName == "#NumberOfPages")
            return new Size
            {
                Height = paint.FontSpacing,
                Width = textBlock.Width
            };

        var lines = WrappedLines(textBlock.Text, (float) textBlock.Width, paint);
        var size = CalcSize(lines, (float) textBlock.Width, paint);

        if ((control as TextBlock).CanGrow)
        {
            var maxLength = lines.Max(l => l.Length);
            var text = lines.Where(s => s.Length == maxLength).First();
            var maxLengthWidth = paint.MeasureText(text);
            if (size.Width < maxLengthWidth)
            {
                lines = WrappedLines(textBlock.Text, (float) size.Width, paint);
                size = CalcSize(lines, (float) size.Width, paint);
            }
        }

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

        SKTextAlign textAlign = textBlock.HorizontalAlignment switch
        {
            HorizontalAlignment.Center => SKTextAlign.Center,
            HorizontalAlignment.Left => SKTextAlign.Left,
            HorizontalAlignment.Right => SKTextAlign.Right,
            _ => SKTextAlign.Left
        };

        SKPaint paint = new SKPaint
        {
            TextSize = (float) textBlock.FontSize,
            Typeface = typeface,
            TextAlign = textAlign
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
        var canvas = context as SKCanvas;
        
        Control[] controls = new Control[2];

        TextBlock textBlock = control.CreateControl() as TextBlock;

        var paint = CreatePaint(control as TextBlock);

        var lines = WrappedLines(textBlock.Text,(float) textBlock.Width, paint);
        var textBuilder = new StringBuilder();

        var novoText = (control as TextBlock).Text.Replace(lines.Last(),"");
        (control as TextBlock).Text = novoText;
        textBlock.Text = lines.Last();
        controls[0] = control;
        controls[1] = textBlock;
        
        return controls;
    }

    void WrapLinesOnRender(TextBlock textBlock, SKCanvas canvas, SKPaint paint)
    {
        var widthToWrapLine = textBlock.Width - textBlock.Border.LeftWidth - textBlock.Border.RightWidth -
                              textBlock.Padding.Left - textBlock.Padding.Right; 
        var oldSize = new Size {Height = textBlock.Size.Height, Width = textBlock.Size.Width};
        textBlock.Size = new Size { Width = widthToWrapLine, Height = textBlock.Size.Height };
        WrapLines(textBlock, canvas,paint);
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
            string textToDraw;
            if (string.IsNullOrEmpty(textBlock.FieldTextFormat))
                textToDraw = wrappedLine;
            else
                textToDraw = string.Format(wrappedLine, textBlock.Text);
            canvas.DrawText(textToDraw, x, y, defPaint);
            y += defPaint.FontSpacing;
        }
    }

    Size CalcSize(IList<string> wrapLines, float lineLengthLimit, SKPaint paint)
    {
//         // define the surface properties
//         var info = new SKImageInfo(256, 256 );
//
// // construct a new surface
//         var surface = SKSurface.Create(info);
//
// // get the canvas from the surface
//         var canvas = surface.Canvas;
//
// // draw on the canvas ...
//
         var y = 0.0f;
         var width = 0.0f;
        
        foreach (var wrapLine in wrapLines)
        {
            y += paint.FontSpacing;
            width = Math.Max(paint.MeasureText(wrapLine), width);
        }

        return new Size
        {
            Width = width,
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