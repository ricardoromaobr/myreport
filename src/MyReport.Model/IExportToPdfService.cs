namespace MyReport.Model;

public interface IExportToPdfService
{
    byte[] ExportToPdf(string path, IList<Page> pages, IReportRenderer reportRenderer, Report report);
}