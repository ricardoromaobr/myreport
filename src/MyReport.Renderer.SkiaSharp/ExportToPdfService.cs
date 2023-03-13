using System.Text.Json;
using MyReport.Model;
using SkiaSharp;

namespace MyReport.Renderer.SkiaSharp;

public class ExportToPdfService : IExportToPdfService
{
    public byte[] ExportToPdf(string path, IList<Page> pages, IReportRenderer reportRenderer, Report report)
    {
        if (pages.Count == 0)
            return null;
        
        var stream = SKFileWStream.OpenStream(path);
        var document = SKDocument.CreatePdf(stream);
        
        foreach (var page in pages)
        {
            var graphicsContext = document.BeginPage((float)report.Width, (float) report.Height);
            reportRenderer.Context = graphicsContext;
            //AjusteControlSize(page);
            reportRenderer.RenderPage(page);
            document.EndPage();
        }
        document.Close();

        return null;
    }

    private void AjusteControlSize(Page page)
    {
        var unitPoint = 1 / 72;

        foreach (var control in page.Controls)
        {
            control.Size = new Size
            {
                Width = control.Size.Width * unitPoint, Height = control.Size.Height * unitPoint
            };
        } 
    }
}