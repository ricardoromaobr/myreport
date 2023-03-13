using MyReport.Model;
using MyReport.Model.Controls;
using SkiaSharp;

namespace MyReport.Renderer.SkiaSharp;

public class LineRenderer : IControlRenderer
{
    public void Render(object context, Control control)
    {
        SKCanvas canvas = context as SKCanvas;
        Line line = control as Line;
        
        SKPoint p1 = new SKPoint ((float)line.Location.X ,(float)line.Location.Y);
        SKPoint p2 = new SKPoint ((float)line.End.X,(float) line.End.Y);

        SKPaint paint = new SKPaint
        {
           Style = SKPaintStyle.StrokeAndFill,
            Color = new SKColor((byte) line.BackgroundColor.R, (byte) line.BackgroundColor.G,
                (byte) line.BackgroundColor.B, (byte) line.BackgroundColor.A),
            StrokeWidth =(float) line.LineWidth,
            IsAntialias = true
        };
        
        SKPaint framePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = (float) line.LineWidth,
            Color = new SKColor((byte) line.BackgroundColor.R, (byte) line.BackgroundColor.G,
                (byte) line.BackgroundColor.B, (byte) line.BackgroundColor.A)
        };
        
        canvas.DrawLine(p1,p2,framePaint);
    }

    public Size Measure(object context, Control control)
    {
        Line line = control as Line;

        var height = line.Height;

        if (line.Location.Y == line.End.Y)
            height = line.LineWidth;
        
        return new Size
        {
            Height = height,
            Width = line.Width
        };
    }

    public double Dpi { get; set; }
    public bool DesignMode { get; set; }
    public Control[] BreakOffControlAtMostAtHeight(object context, Control control, double height)
    {
        Control[] controls = new Control[2];
        var first = control.CreateControl () as Line;
        var second = control.CreateControl () as Line;
        double newX = 0;
						
        if ( first.Location.X != first.End.X ) {
            if ( first.Location.Y > first.End.Y ) {
                newX = CalculateXAtYZero(first.End.X,height,first.Location.X,-(first.Height - height));
                first.Location = new MyReport.Model.Point(newX, first.End.Y + height);										
                double deltaW = second.End.X - newX;				
                second.Left -= deltaW;					
                second.Top = 0;
                second.Location = new MyReport.Model.Point(second.Location.X + deltaW , second.Location.Y - height);	
            } else if (first.Location.Y < first.End.Y) {
					
                newX = CalculateXAtYZero(first.Location.X,height,first.End.X,-(first.Height - height));
                first.End = new MyReport.Model.Point(newX, first.Location.Y + height);										
                double deltaW = second.Location.X - newX;				
                second.Left -= deltaW;					
                second.Top = 0;
                second.End = new MyReport.Model.Point(second.End.X + deltaW , second.End.Y - height);			
            }
        } else {
            if ( first.Location.Y > first.End.Y ) {
                first.Location = new MyReport.Model.Point(first.Location.X, first.End.Y + height);		
                second.Top = 0;
                second.Location = new MyReport.Model.Point(second.Location.X, second.Location.Y - height);
            } else if (first.Location.Y < first.End.Y) {
                first.End = new MyReport.Model.Point(first.End.X, first.Location.Y + height);		
                second.Top = 0;
                second.End = new MyReport.Model.Point(second.End.X , second.End.Y - height);
            }
        }
        controls [0] = first;			
        controls [1] = second;
        return controls;
    }
    
    /// <summary>
    /// Calculates the X at Y = 0.
    /// </summary>
    /// <returns>
    /// The X at Y = 0.
    /// </returns>
    /// <param name='x1'>
    /// x1
    /// </param>
    /// <param name='y1'>
    /// y1.
    /// </param>
    /// <param name='x2'>
    /// x2.
    /// </param>
    /// <param name='y2'>
    /// y2.
    /// </param>
    static double CalculateXAtYZero (double x1,double y1, double x2, double y2)
    {	
        if (y1 == y2) {
            return x1;				
        } else {			
            return x1 - (((y1 * x1) - (y1 * x2)) / (y1 - y2));
        }
    }
}