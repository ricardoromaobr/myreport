using MyReport.Model;
using MyReport.Model.Controls;
using MyReport.Model.Engine;

namespace MyReport.FluentDesigner;

public class ReportEngineBuilder
{
    private Action<Report, IReportRenderer> _reportEngineConfigure;
    private IReportRenderer _reportRenderer;
    private Report _report;
    private Action<IReportRenderer> _addControlRenderer;

    public static ReportEngineBuilder Create()
        => new ReportEngineBuilder();

    public ReportEngineBuilder ReportEngineConfigure(Action<Report, IReportRenderer> reportEngineConfigure)
    {
        _reportEngineConfigure = reportEngineConfigure;
        return this;
    }

    public ReportEngineBuilder AddReportRenderer(IReportRenderer reportRenderer)
    {
        _reportRenderer = reportRenderer;
        return this;
    }

    public ReportEngineBuilder AddReport(Report report)
    {
        _report = report;
        return this;
    }

    public ReportEngineBuilder AddControlRenderer(Action<IReportRenderer> addControlRenderer)
    {
        _addControlRenderer = addControlRenderer;
        return this;
    }

    public ReportEngine Build()
    {
        if (_report is null)
            _report = new Report();

        // if (_reportRenderer is null)
        //     throw new ArgumentException("ReportRenderer não definido! Use: AddReportRenderer");

        _reportEngineConfigure?.Invoke(_report, _reportRenderer);
        _addControlRenderer?.Invoke(_reportRenderer);

        var reportEngine = new ReportEngine(_report, _reportRenderer);
        
        return reportEngine;
    }
}