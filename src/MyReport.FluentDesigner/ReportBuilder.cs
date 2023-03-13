using MyReport.Model;
using MyReport.Model.Controls;
using MyReport.Model.Data;

namespace MyReport.FluentDesigner;

public class ReportBuilder
{
    private readonly Report _report;
    private Action<ReportHeaderSection> _reportHeaderSectionBuilder;
    private Action<PageHeaderSection> _pageHeaderSectionBuilder;
    private Action<DetailSection> _detailReportBuilder;
    private Action<PageFooterSection> _pageFooterBuilder;
    private Action<List<GroupHeaderSection>> _groupHeaderBuilder;
    private Action<Thickness> _marginBuilder;
    private Action<Report> _datasourceBuilder;
    private object _groupFooterBuilder;
    private Action<List<Field>> _parameterBuilder;
    private Action<Report> _datasourceFromScriptBuilder;


    public static ReportBuilder Create()
    {
        return new ReportBuilder();
    }
    public ReportBuilder()
    {
        _report = new Report();
    }

    public Report build()
    {
        if (UsingDataScript && string.IsNullOrEmpty(_report.DataScript))
        {
            _datasourceFromScriptBuilder(_report);
        }
        else
            _datasourceBuilder?.Invoke(_report);
        _parameterBuilder?.Invoke(_report.Parameters);
        _marginBuilder?.Invoke(_report.Margin);
        _reportHeaderSectionBuilder?.Invoke(_report.ReportHeaderSection);
        _pageHeaderSectionBuilder?.Invoke(_report.PageHeaderSection);
        _detailReportBuilder?.Invoke(_report.DetailSection);
        _pageFooterBuilder?.Invoke(_report.PageFooterSection);
        _groupHeaderBuilder?.Invoke(_report.GroupHeaderSections);

        return _report;
    }

    public ReportBuilder SetUsingDataScript(bool usingDataScript)
    {
        UsingDataScript = usingDataScript;
        return this;
    }

    public bool UsingDataScript { get; private set; }

    public ReportBuilder BuilderReportHeader(Action<ReportHeaderSection> reportHeaderSectionBuilder)
    {
        _reportHeaderSectionBuilder = reportHeaderSectionBuilder;
        return this;
    }

    public ReportBuilder BuilderPageHeader(Action<PageHeaderSection> pageHeaderSectionBuilder)
    {
        _pageHeaderSectionBuilder = pageHeaderSectionBuilder;
        return this;
    }

    public ReportBuilder BuilderDetail(Action<DetailSection> detailReportBuilder)
    {
        _detailReportBuilder = detailReportBuilder;
        return this;
    }

    public ReportBuilder BuilderPageFooter(Action<PageFooterSection> pageFooterBuilder)
    {
        _pageFooterBuilder = pageFooterBuilder;
        return this;
    }

    public ReportBuilder BuilderGroupHeader(Action<List<GroupHeaderSection>> groupHeaderBuilder)
    {
        _groupHeaderBuilder = groupHeaderBuilder;
        return this;
    }

    public ReportBuilder BuilderGroupFooter(Action<List<GroupFooterSection>> groupFooterBuilder)
    {
        _groupFooterBuilder = groupFooterBuilder;
        return this;
    }

    public ReportBuilder BuilderParameter(Action<List<Field>> parameterBuilder)
    {
        _parameterBuilder = parameterBuilder;
        return this;
    }

    public ReportBuilder SetTitle(string title)
    {
        _report.Title = title;
        return this;
    }

    public ReportBuilder SetDataScript(string dataScript)
    {
        _report.DataScript = dataScript;
        return this;
    }

    public ReportBuilder SetPageWidth(double width)
    {
        _report.Width = width;
        return this;
    }

    public ReportBuilder SetPageHeight(double height)
    {
        _report.Height = height;
        return this;
    }

    public ReportBuilder SetMargin(Action<Thickness> marginBuilder)
    {
        _marginBuilder = marginBuilder;
        return this;
    }

    public ReportBuilder SetDatasource(Action<Report> datasourceBuilder)
    {
        _datasourceBuilder = datasourceBuilder;
        return this;
    }

    public ReportBuilder DatasourceFromScriptBuilder(Action<Report> datasourceFromScriptBuilder)
    {
        _datasourceFromScriptBuilder = datasourceFromScriptBuilder;
        return this;
    }
}