using System.IO;
using System.Text.RegularExpressions;
using BADEAPORTAL.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BADEAPORTAL.Services
{
    public sealed class QuestPdfMemoService : IMemoPdfService
    {
        public byte[] GenerateMemoPdf(MemoPdfRequest request)
        {
            // Very simple HTML -> plain text. You can replace this with your
            // existing HTML-to-Blocks logic from the memo generator project.
            var plainTextBody = StripHtml(request.BodyHtml);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(TextStyle.Default.FontSize(11));

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text("MEMO")
                            .FontSize(18)
                            .Bold()
                            .AlignCenter();

                        col.Item().LineHorizontal(1);

                        col.Item().Column(header =>
                        {
                            header.Spacing(3);
                            header.Item().Text(text =>
                            {
                                text.Span("To: ").SemiBold();
                                text.Span(request.To);
                            });

                            if (!string.IsNullOrWhiteSpace(request.Through))
                            {
                                header.Item().Text(text =>
                                {
                                    text.Span("Through: ").SemiBold();
                                    text.Span(request.Through);
                                });
                            }

                            header.Item().Text(text =>
                            {
                                text.Span("From: ").SemiBold();
                                text.Span(request.From);
                            });

                            header.Item().Text(text =>
                            {
                                text.Span("Subject: ").SemiBold();
                                text.Span(request.Subject);
                            });

                            header.Item().Text(text =>
                            {
                                text.Span("Classification: ").SemiBold();
                                text.Span(request.Classification);
                            });

                            header.Item().Text(text =>
                            {
                                text.Span("Date: ").SemiBold();
                                text.Span(request.CreatedAtUtc.ToString("yyyy-MM-dd"));
                            });

                            header.Item().Text(text =>
                            {
                                text.Span("Prepared by: ").SemiBold();
                                text.Span(request.CreatedByName);
                            });
                        });

                        col.Item().LineHorizontal(0.5f);

                        col.Item().Text(plainTextBody)
                            .FontSize(11);
                    });
                });
            });

            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove images explicitly, just in case
            html = Regex.Replace(html, "<img[^>]*>", string.Empty, RegexOptions.IgnoreCase);

            // Kill all tags
            html = Regex.Replace(html, "<.*?>", string.Empty);

            return System.Net.WebUtility.HtmlDecode(html).Trim();
        }
    }
}
