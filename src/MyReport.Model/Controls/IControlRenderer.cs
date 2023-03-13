namespace MyReport.Model.Controls;

public interface IControlRenderer
{
    void Render(object context, Control control);
    Size Measure(object context, Control control);
    
    double Dpi { get; set; }
    
    bool DesignMode { get; set; }
    Control[] BreakOffControlAtMostAtHeight(object context, Control control, double height);
}