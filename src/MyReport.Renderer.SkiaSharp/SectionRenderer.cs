using MyReport.Model;
using MyReport.Model.Controls;
using SkiaSharp;

namespace MyReport.Renderer.SkiaSharp;

public class SectionRenderer: IControlRenderer
{
    public void Render(object context, Control control)
    {
        SKCanvas canvas = context as SKCanvas;
        Section section = control as Section;
        SKRect borderRect;
        
        canvas.Save ();
        
        borderRect = new SKRect((float)section.Location.X, (float)section.Location.Y, (float)section.Width, (float)section.Bottom);
        canvas.ClipRect(borderRect);
        //borderRect = new SKRect((float)section.Location.X, (float)section.Location.Y, (float)section.Width, (float)section.Height);
        
        var paint = new SKPaint
        {
            Style = SKPaintStyle.StrokeAndFill,
            Color = new SKColor((byte) section.BackgroundColor.R, (byte) section.BackgroundColor.G,
                (byte) section.BackgroundColor.B, (byte) section.BackgroundColor.A)
        };
        
        canvas.DrawRect(borderRect, paint);
			 		
        canvas.Restore (); 
    }

    public Size Measure(object context, Control control)
    {
        Section section = control as Section;
        SKRect borderRect = new SKRect((float)section.Location.X, (float)section.Location.Y,(float) section.Width,(float) section.Height);
        return new MyReport.Model.Size(borderRect.Width,borderRect.Height);
    }

    public double Dpi { get; set; }
    public bool DesignMode { get; set; }
    public Control[] BreakOffControlAtMostAtHeight(object context, Control control, double height)
    {
        Control[] controls = new Control[2];
        var newControl = control.CreateControl();
        var newControl1 = control.CreateControl();
        newControl.Height = height;
        newControl1.Height = control.Height - height;			
        newControl1.Top = 0;
        controls[1] = newControl1;
        controls[0] = newControl;
        return controls;
    }
}