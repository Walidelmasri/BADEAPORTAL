using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BADEAPORTAL.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BADEAPORTAL.Services
{
    public sealed class MemoPdfDocument : IDocument
    {
        private readonly MemoPdfRequest _request;
        private readonly byte[] _bannerImage;
        private readonly byte[] _footerImage;

        public MemoPdfDocument(MemoPdfRequest request, byte[] bannerImage, byte[] footerImage)
        {
            _request = request ?? throw new ArgumentNullException(nameof(request));
            _bannerImage = bannerImage ?? throw new ArgumentNullException(nameof(bannerImage));
            _footerImage = footerImage ?? throw new ArgumentNullException(nameof(footerImage));
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);

                page.MarginLeft(45);
                page.MarginRight(45);
                page.MarginTop(28);
                page.MarginBottom(28);

                page.DefaultTextStyle(ts =>
                    ts.FontFamily("Tajawal")
                      .FontSize(11)
                      .FontColor(Colors.Grey.Darken4)
                );

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(col =>
            {
                col.Item()
                   .PaddingBottom(10)
                   .Image(_bannerImage)
                   .FitWidth();

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Memo")
                        .FontSize(18)
                        .SemiBold()
                        .FontColor(Colors.Black);

                    row.ConstantItem(160)
                       .AlignRight()
                       .Text(_request.CreatedAtUtc.ToString("dd MMM yyyy"))
                       .FontSize(10)
                       .FontColor(Colors.Grey.Darken1);
                });

                col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(10);
            });
        }

        private void ComposeContent(IContainer container)
        {
            container.Column(col =>
            {
                col.Spacing(10);

                col.Item().Element(ComposeMemoHeader);

                col.Item().PaddingTop(4).Element(body =>
                {
                    body.Column(b =>
                    {
                        b.Spacing(6);

                        foreach (var para in HtmlToParagraphs(_request.BodyHtml))
                        {
                            b.Item()
                             .Text(para)
                             .FontSize(11)
                             .LineHeight(1.35f);
                        }
                    });
                });

col.Item().PaddingTop(14).Row(row =>
{
    row.RelativeItem().Text(t =>
    {
        t.DefaultTextStyle(
            TextStyle.Default
                .FontFamily("Tajawal")
                .FontSize(10)
                .FontColor(Colors.Grey.Darken2)
        );

        t.Span("Prepared by: ").SemiBold();
        t.Span(_request.CreatedByName ?? string.Empty);
    });
});

            });
        }

        private void ComposeMemoHeader(IContainer container)
        {
            container
                .Border(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Background(Colors.Grey.Lighten5)
                .Padding(14)
                .Column(col =>
                {
                    col.Spacing(6);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text(_request.Subject ?? string.Empty)
                            .FontSize(13)
                            .SemiBold()
                            .FontColor(Colors.Black);
                    });

                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(90);
                            c.RelativeColumn();
                        });

                        AddRow(table, "To", _request.To);
                        if (!string.IsNullOrWhiteSpace(_request.Through))
                            AddRow(table, "Through", _request.Through!);

                        AddRow(table, "From", _request.From);
                        AddRow(table, "Subject", _request.Subject);
                        AddRow(table, "Classification", _request.Classification);
                    });
                });
        }

        private static void AddRow(TableDescriptor table, string label, string value)
        {
            table.Cell()
                 .PaddingVertical(3)
                 .Text(label)
                 .SemiBold()
                 .FontColor(Colors.Grey.Darken2);

            table.Cell()
                 .PaddingVertical(3)
                 .Text(value ?? string.Empty)
                 .FontColor(Colors.Black);
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                col.Item()
                   .PaddingTop(8)
                   .Image(_footerImage)
                   .FitWidth();
            });
        }

        private static IEnumerable<string> HtmlToParagraphs(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                yield break;

            html = Regex.Replace(html, "<img[^>]*>", "", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, @"<\s*br\s*/?\s*>", "\n", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"</\s*(p|div|li)\s*>", "\n", RegexOptions.IgnoreCase);

            html = Regex.Replace(html, "<.*?>", string.Empty);

            var text = System.Net.WebUtility.HtmlDecode(html);
            text = text.Replace("\r\n", "\n");

            foreach (var line in text.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var trimmed = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmed))
                    yield return trimmed;
            }
        }
    }
}
