using MyReport.Model;
using MyReport.Model.Engine;

namespace MyReport.FluentDesigner;

public class ReportRuntime
{
    private readonly Report _report;
    private readonly IReportRenderer _reportRender;
    private readonly ReportEngine _reportEngine;

    public ReportRuntime(Report report, IReportRenderer reportRenderer)
    {
        _report = report;
        _reportRender = reportRenderer;
        _reportEngine = new ReportEngine(report, reportRenderer);
    }

    public Report Report => _report;
    public IReportRenderer ReportRenderer => _reportRender;
    public IExportToPdfService ExportToPdfService { get; set; }

    private bool ReportProcessed { get; set; }

    public ReportRuntime Run()
    {
        _reportEngine.Process();
        ReportProcessed = true;
        return this;
    }

    public void ExportToPdf(string path)
    {
        if (ReportProcessed)
            ExportToPdfService.ExportToPdf(path, _report.Pages, _reportRender, _report);
    }
}