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
        canvas.Save();
        
        borderRect = new SKRect((float) image.Location.X, (float) image.Location.Y, (float) (image.Location.X +image.Width),
            (float) image.Bottom);
        
        canvas.ClipRect(borderRect);


        var paint = new SKPaint
        {
            Color = new SKColor((byte) image.BackgroundColor.R, (byte) image.BackgroundColor.G,
                (byte) image.BackgroundColor.B, (byte) image.BackgroundColor.A)
        };

        var bitmap = SKBitmap.Decode(image.Data);
        var point = new SKPoint((float) (image.Location.X + image.Border.LeftWidth), 
            (float) (image.Location.Y + image.Border.TopWidth));

        paint = new SKPaint
        {
            Color = SKColors.Yellow
        };
    

        canvas.DrawRect(borderRect, paint);
    canvas.DrawBitmap(bitmap, point);
        
        
       
    


    canvas.Restore (); 
    }

    public Size Measure(object context, Control control)
    {
        Image image = control as Image;
        
        
        SKRect borderRect = new SKRect ((float)image.Location.X, (float)image.Location.Y, 
            (float)(image.Location.X + image.Width + image.Border.LeftWidth + image.Border.RightWidth), 
            (float)(image.Bottom + image.Border.TopWidth + image.Border.BottomWidth));
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