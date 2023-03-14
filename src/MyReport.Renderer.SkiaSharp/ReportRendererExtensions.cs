using MyReport.Model.Controls;

namespace MyReport.Renderer.SkiaSharp;

public static class ReportRendererExtensions
{
    public static void AddDefaultRenderers(this ReportRenderer reportRenderer)
    {
        reportRenderer.RegisterRenderer(typeof(Line), new LineRenderer());
        reportRenderer.RegisterRenderer(typeof(TextBlock), new TextBlockRenderer());
        reportRenderer.RegisterRenderer(typeof(Image), new ImageRenderer());
        SectionRenderer sr = new SectionRenderer();
        reportRenderer.RegisterRenderer(typeof(ReportHeaderSection), sr);
        reportRenderer.RegisterRenderer(typeof(ReportFooterSection), sr);
        reportRenderer.RegisterRenderer(typeof(PageHeaderSection), sr);
        reportRenderer.RegisterRenderer(typeof(PageFooterSection), sr);
        reportRenderer.RegisterRenderer(typeof(DetailSection), sr);
    }
}