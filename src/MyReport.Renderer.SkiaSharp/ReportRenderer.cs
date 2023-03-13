using MyReport.Model;
using MyReport.Model.Controls;

namespace MyReport.Renderer.SkiaSharp;

public class ReportRenderer : IReportRenderer
{
    private Dictionary<Type, IControlRenderer> _controlRenderers;
    private object _context;


    public ReportRenderer()
    {
        _controlRenderers = new Dictionary<Type, IControlRenderer>();
    }
    
    public object Context
    {
        get => _context;
        set => _context = value;
    }

    public void RegisterRenderer(Type type, IControlRenderer renderer)
    {
        _controlRenderers.Add(type, renderer);
    }

    public void RenderPage(Page page)
    {
        for (int i = 0; i < page.Controls.Count; i++) {
            var control = page.Controls[i];
            if(control.IsVisible)
                RenderControl (control);				 				
        }
    }
    public Size MeasureControl(Control control)
    {
        Type controlType = control.GetType();
        if(_controlRenderers.ContainsKey(controlType)){
            var renderer = _controlRenderers[controlType] as IControlRenderer;                
            return renderer.Measure(_context,control);								
        }
        return default(Size);
    }

    public void RenderControl(Control control)
    {
        Type controlType = control.GetType();
        if(_controlRenderers.ContainsKey(controlType)){
            var renderer = _controlRenderers[controlType] as IControlRenderer;							
            renderer.Render(_context,control);								
        }
    }
    public Control[] BreakOffControlAtMostAtHeight(Control control, double height)
    {
        Control[] controls = new Control[2];
			
        Type controlType = control.GetType();
        if(_controlRenderers.ContainsKey(controlType)){
            var renderer = _controlRenderers[controlType] as IControlRenderer;							
            controls = renderer.BreakOffControlAtMostAtHeight(_context,control,height);
        }
			
        return controls;
    }

}