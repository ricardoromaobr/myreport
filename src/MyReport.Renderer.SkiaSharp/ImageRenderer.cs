using System.Drawing;
using System.Reflection.Metadata;
using MyReport.Model.Controls;
using SkiaSharp;
using Size = MyReport.Model.Size;

namespace MyReport.Renderer.SkiaSharp;

public class ImageRenderer : IControlRenderer
{
    public void Render(object context, Control control)
    {
        SKCanvas canvas = context as SKCanvas;
        Image image = control as Image;
        SKRect borderRect;
        
        canvas.Save();

        borderRect = new SKRect((float) image.Location.X, (float) image.Location.Y,
            (float) (image.Location.X + image.Width),
            (float) image.Bottom);

        canvas.ClipRect(borderRect);


        var bitmap = SKBitmap.Decode(image.Data);
        
        var newSize = new SKSizeI((int) (image.Width - image.Border.LeftWidth - image.Border.RightWidth),
            (int) (image.Height - image.Border.BottomWidth - image.Border.TopWidth));

        var point = new SKPoint((float) (image.Location.X + image.Border.LeftWidth),
            (float) (image.Location.Y + image.Border.TopWidth));

        var paint = new SKPaint
        {
            Color = new SKColor((byte) image.BackgroundColor.R, (byte) image.BackgroundColor.G,
                (byte) image.BackgroundColor.B, (byte) image.BackgroundColor.A)
        };

        canvas.DrawRect(borderRect, paint);

        paint = new SKPaint
        {
            Color = new SKColor((byte) image.Border.Color.R, (byte) image.Border.Color.G,
                (byte) image.Border.Color.B, (byte) image.Border.Color.A),
            StrokeWidth = (float) image.Border.TopWidth
        };

        SKPoint p1 = new((float) image.Location.X, (float) image.Location.Y);
        SKPoint p2 = new((float) (image.Location.X + image.Width), (float) image.Location.Y);
        
        if (image.Border.TopWidth > 0)
            canvas.DrawLine(p1, p2, paint);

        if (image.Border.LeftWidth > 0)
        {
            paint.StrokeWidth = (float)image.Border.LeftWidth;

            p1.X = (float)image.Location.X;
            p1.Y = (float) image.Location.Y;

            p2.X = p1.X;
            p2.Y = (float) image.Bottom;
            canvas.DrawLine(p1,p2,paint);
        }

        if (image.Border.RightWidth > 0)
        {
            paint.StrokeWidth = (float)image.Border.RightWidth;

            p1.X = (float)(image.Location.X + image.Width);
            p1.Y = (float) image.Location.Y;

            p2.X = p1.X;
            p2.Y = (float) image.Bottom;
            canvas.DrawLine(p1,p2,paint);
        }

        if (image.Border.BottomWidth > 0)
        {
            paint.StrokeWidth = (float) image.Border.BottomWidth;
            p1.X = (float) image.Location.X;
            p1.Y = (float) image.Bottom;

            p2.Y = p1.Y;
            p2.X = (float) (image.Location.X + image.Width);
            canvas.DrawLine(p1,p2,paint);
        }

        var resizedImage = bitmap.Resize(newSize, SKFilterQuality.High);
        canvas.DrawBitmap(resizedImage, point);


        canvas.Restore();
    }

    public Size Measure(object context, Control control)
    {
        Image image = control as Image;


        SKRect borderRect = new SKRect((float) image.Location.X, (float) image.Location.Y,
            (float) (image.Location.X + image.Width),
            (float) (image.Bottom));
        return new MyReport.Model.Size(borderRect.Width, borderRect.Height);
    }

    public double Dpi { get; set; }
    public bool DesignMode { get; set; }

    public Control[] BreakOffControlAtMostAtHeight(object context, Control control, double height)
    {
        Control[] controls = new Control[2];
        var newControl = control.CreateControl();
        var newControl1 = control.CreateControl() as Image;
        newControl.Height = height;
        newControl1.Height = control.Height - height;
        newControl1.Offset = new MyReport.Model.Point(0, -height);
        newControl1.Top = 0;
        controls[1] = newControl1;
        controls[0] = newControl;
        return controls;
    }
}