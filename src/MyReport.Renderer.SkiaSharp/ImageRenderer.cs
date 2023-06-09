using System.Drawing;
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
        canvas.Save ();
        borderRect = new SKRect ((float)image.Location.X, (float)image.Location.Y,(float) image.Width, (float)image.Height);
        canvas.ClipRect(borderRect);
        borderRect = new SKRect ((float)image.Location.X, (float)image.Location.Y, (float)image.Width, (float)image.Height);
        
        var paint = new SKPaint
        {
            Color = new SKColor((byte) image.BackgroundColor.R, (byte) image.BackgroundColor.G,
                (byte) image.BackgroundColor.B, (byte) image.BackgroundColor.A)
        };
        
        //TODO: Renderer the image
        
        
        canvas.DrawRect(borderRect, paint);
        canvas.Restore (); 
    }

    public Size Measure(object context, Control control)
    {
        Image image = control as Image;
        SKRect borderRect = new SKRect ((float)image.Location.X, (float)image.Location.Y, (float)image.Width, (float)image.Height);
        return new MyReport.Model.Size(borderRect.Width,borderRect.Height);
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
        newControl1.Offset = new MyReport.Model.Point(0,-height);
        newControl1.Top = 0;
        controls[1] = newControl1;
        controls[0] = newControl;
        return controls;
    }
}