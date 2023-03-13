// See https://aka.ms/new-console-template for more information

using System.Net;
using MyReport.FluentDesigner;
using MyReport.Model;
using MyReport.Model.Controls;
using MyReport.Model.Data;
using MyReport.Model.Engine;
using MyReport.Renderer.SkiaSharp;

var report = ReportBuilder
     .Create()
     .BuilderReportHeader(headerSecion =>
     {
         headerSecion.Height = 8;
         var line = new Line();
         line.Location = new Point(0, 2);
         line.End = new Point(538, 2);
         line.LineWidth = 1;
         line.BackgroundColor = new Color(0, 0, 255, 255);

         var line2 = new Line();
         line2.LineWidth = 1;
         line2.Location = new Point(0, headerSecion.Height - line2.LineWidth);
         line2.End = new Point(538, headerSecion.Height - line2.LineWidth);

         TextBlock textBlock = new();
         textBlock.Location = new Point(0, 0);
         textBlock.Width = 250;
         textBlock.Text = "Esse um teste para valer apena o trabalho já feito até aqui!!!!";

         headerSecion.Controls.Add(line);
         headerSecion.Controls.Add(textBlock);
         headerSecion.Controls.Add(line2);
         headerSecion.BackgroundColor = new(0, 0, 255,128);
     })
     .BuilderPageHeader(pageHeader =>
     {
         
         pageHeader.Height = 8;
         var titulo = new TextBlock();
         
         titulo.Location = new Point(50,0);
         titulo.FieldKind = FieldKind.Data;
         titulo.Text = "Teste titulo, caçador gato, jeito";
         titulo.Width = 250;
         pageHeader.Controls.Add(titulo);
         pageHeader.BackgroundColor = new Color(255, 0, 0, 50);

         var titulo2 = new TextBlock();
         titulo2.Location = new Point(253, 0);
         titulo2.Width = 250;
         titulo2.Text = "NOVO MUNDO";
         titulo2.FontSize = 72;
         var line = new Line();
         line.Location = new Point(0, 0);
         line.End = new Point(538, 0);
         line.BackgroundColor = new Color(255, 0, 0, 255);
         
         var line2 = new Line();
         line2.LineWidth = 1;
         line2.Location = new Point(0, pageHeader.Height - line2.LineWidth);
         line2.End = new Point(538, pageHeader.Height - line2.LineWidth);
         line2.BackgroundColor = new Color(0, 0, 0, 255);
         
         var line3 = new Line();
         line3.LineWidth = 1;
         line3.Location = new Point(0, 10);
         line3.End = new Point(538, 50);
         line3.BackgroundColor = new Color(0, 0, 0, 255);
         
         pageHeader.Controls.Add(line);
         pageHeader.Controls.Add(line2);
         pageHeader.Controls.Add(titulo2);
         pageHeader.Controls.Add(line3);
         pageHeader.BackgroundColor = new Color(0, 255, 0, 255);

     }) 
     .BuilderDetail(detail =>
     {
         detail.Height = 7;
         detail.BackgroundColor = new Color(50, 50, 0, 128);
         
         var textBlock = new TextBlock();
         textBlock.FieldKind = FieldKind.Data;
         textBlock.FieldName = "Nome";
         textBlock.Location = new Point(0, 0);
         textBlock.FontName = "Arial";
         textBlock.Width = 70;
         
         var textBlock2 = new TextBlock();
         textBlock2.FieldKind = FieldKind.Data;
         textBlock2.FieldName = "Idade";
         textBlock2.Location = new Point(280, 0);
         textBlock2.FontColor = new Color(255, 0, 0);
         textBlock2.Width = 150;

         var testeAny = new TextBlock();
         testeAny.Text = "qualquer coisa";
         testeAny.Location = new Point(320, 0);
         testeAny.Width = 50;
         testeAny.FieldKind = FieldKind.Data;
         
         var testeAny2 = new TextBlock();
         testeAny2.Text = "roberto";
         testeAny2.Location = new Point(380, 0);
         testeAny2.Width = 100;
         testeAny2.FieldKind = FieldKind.Data;
         
         detail.Controls.Add(testeAny2);
         detail.Controls.Add(testeAny);
         detail.Controls.Add(textBlock);
         detail.Controls.Add(textBlock2);
         
     })
     .SetDatasource(report =>
     {
         List<Pessoa> datasource = new List<Pessoa>
         {
             new Pessoa
             {
                 Nome = "Ricardo Romão Soare",
                 Idade = 47
             }, 
             new Pessoa
             {
                 Nome = "Rogéria Silva Abreu Soares", 
                 Idade = 42
             },
             new Pessoa
             {
                 Nome = "Gabriele Silva Romão", 
                 Idade = 5
             },
             new Pessoa
             {
                 Nome = "Heitor Silva Romão", 
                 Idade = 4
             }
         };

         report.DataSource = datasource;
     }) 
     .BuilderPageFooter(pageFooter =>
     {
         var numPage = new TextBlock();
         numPage.FieldKind = FieldKind.Expression;
         numPage.FieldName = "#PageNumber";
         numPage.Location = new Point(pageFooter.Width - 50, 0);
         
         pageFooter.Controls.Add(numPage);
     })
     .build();



var reportEngine = ReportEngineBuilder
    .Create()
    .AddReport(report)
    .AddReportRenderer(new ReportRenderer())
    .ReportEngineConfigure((report, reportRenderer) =>
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
    })
    .Build();

 ReportRuntimeBuilder
    .Create()
    .AddReport(report)
    .AddReportRenderer(() =>
    {
        ReportRenderer reportRenderer = new ReportRenderer();
        reportRenderer.RegisterRenderer(typeof(Line), new LineRenderer());
        reportRenderer.RegisterRenderer(typeof(TextBlock), new TextBlockRenderer());
        reportRenderer.RegisterRenderer(typeof(Image), new ImageRenderer());
        SectionRenderer sr = new SectionRenderer();
        reportRenderer.RegisterRenderer(typeof(ReportHeaderSection), sr);
        reportRenderer.RegisterRenderer(typeof(ReportFooterSection), sr);
        reportRenderer.RegisterRenderer(typeof(PageHeaderSection), sr);
        reportRenderer.RegisterRenderer(typeof(PageFooterSection), sr);
        reportRenderer.RegisterRenderer(typeof(DetailSection), sr);
        return reportRenderer;
    })
    .ConfireExportToPdf(() => new ExportToPdfService())
    .Build()
    .Run()
    .ExportToPdf("teste.pdf");


class Pessoa
{
    public string Nome { get; set; }
    public int Idade { get; set; }
}
     
     