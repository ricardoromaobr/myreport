using MyReport.Model;

namespace MyReport.FluentDesigner;

public class ReportRuntimeBuilder
{
    private Report _report;
    private IReportRenderer _reportRenderer;
    private Func<Report> _addReport;
    private Func<IReportRenderer> _addReportRendeter;
    private Func<IExportToPdfService> _configExportToPdf;
    private Action _addDefaultControlRenderer;

    public static ReportRuntimeBuilder Create () => new ReportRuntimeBuilder();
    
    public ReportRuntimeBuilder AddReport(Report report)
    {
        _report = report;
        return this;
    }

    public ReportRuntimeBuilder AddReport(Func<Report> addReport)
    {
        _addReport = addReport;
        return this;
    }

    public ReportRuntimeBuilder AddReportRenderer(IReportRenderer reportRenderer)
    {
        _reportRenderer = reportRenderer;
        return this;
    }

    public ReportRuntimeBuilder AddReportRenderer(Func<IReportRenderer> addReportRenderer)
    {
        _addReportRendeter = addReportRenderer;
        return this;
    }

    public ReportRuntimeBuilder ConfireExportToPdf(Func<IExportToPdfService> configExportToPdf)
    {
        _configExportToPdf = configExportToPdf;
        return this;
    }

    public ReportRuntimeBuilder AddDefaultControlsRenderer (IRegisterDefaultRenderers registerDefaultRenderers)
    {
        _addDefaultControlRenderer = () => registerDefaultRenderers.AddRenderers(_reportRenderer);
        return this;
    }

    public ReportRuntime Build()
    {
        var report = _addReport?.Invoke();
        var reportRenderer = _addReportRendeter?.Invoke();

        if (report is not null) _report = report;
        if (reportRenderer is not null) _reportRenderer = reportRenderer;

        var reportRuntime = new ReportRuntime(_report, _reportRenderer);
        reportRuntime.ExportToPdfService = _configExportToPdf?.Invoke()!;
        _addDefaultControlRenderer?.Invoke();
        
        return reportRuntime;
    }
}