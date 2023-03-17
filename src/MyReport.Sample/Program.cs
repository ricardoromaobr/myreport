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

         var x = headerSecion;
         TextBlock textBlock = new();
         textBlock.Location = new Point(headerSecion.Width / 2f, 0);
         textBlock.Width = 250;
         textBlock.Text = "RELATÓRIO";
         textBlock.HorizontalAlignment = HorizontalAlignment.Center;
         headerSecion.Controls.Add(textBlock);
         headerSecion.BackgroundColor = new(0, 0, 255,128);

         var image = new Image();
         var base64String =
             "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";

         image.Data = Convert.FromBase64String(base64String);

         image.Location = new Point(250, 14);
         image.Width = 100;
         image.Height = 100;
         image.Border = new Border(10);
         headerSecion.Controls.Add(image);
     })
     .BuilderPageHeader(pageHeader =>
     {
         
         pageHeader.Height = 8;
         var collumnName = new TextBlock();
         collumnName.Text = "Name";
         collumnName.Location = new Point(0, 0);
         
         var collumnIdade = new TextBlock();
         collumnIdade.Text = "Idade";
         collumnIdade.Location = new Point(280, 0);
         
         pageHeader.Controls.Add(collumnName);
         pageHeader.Controls.Add(collumnIdade);
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
             },
             
             
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

/*

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
    */ 


 ReportRuntimeBuilder
    .Create()
    .AddReport(report)
    .AddReportRenderer(new ReportRenderer())
    .AddDefaultControlsRenderer(new RegisterDefaultRenderers())
    .ConfireExportToPdf(() => new ExportToPdfService())
    .Build()
    .Run()
    .ExportToPdf("teste.pdf");


class Pessoa
{
    public string Nome { get; set; }
    public int Idade { get; set; }
}
     
     