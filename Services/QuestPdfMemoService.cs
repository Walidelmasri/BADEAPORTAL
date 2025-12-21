using System;
using System.IO;
using System.Linq;
using System.Text;
using BADEAPORTAL.Models;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BADEAPORTAL.Services
{
    public sealed class QuestPdfMemoService : IMemoPdfService
    {
        private static bool _fontsRegistered;
        private static readonly object _fontLock = new();

        private readonly IWebHostEnvironment _env;

        public QuestPdfMemoService(IWebHostEnvironment env)
        {
            _env = env;

            EnsureFontsRegistered(_env);
        }

        public byte[] GenerateMemoPdf(MemoPdfRequest request)
        {
            // Build the same "document input" shape as the memo generator
            var memo = new MemoDocument
            {
                MemoNumber = $"Memo-{DateTime.UtcNow:yyyyMMddHHmmss}", // or entity.Id if you prefer
                To = request.To,
                Through = request.Through ?? string.Empty,
                From = request.From,
                Subject = request.Subject,
                Classification = request.Classification,
                Date = request.CreatedAtUtc,
                PreparedBy = request.CreatedByName,
                BodyHtml = request.BodyHtml,
                BannerImageBytes = ReadWwwrootBytes("images/memo-banner.png"),
                FooterImageBytes = ReadWwwrootBytes("images/memo-footer.png")
            };

            var pdf = BuildPdf(memo);
            return pdf;
        }

        private byte[] BuildPdf(MemoDocument doc)
        {
            // Important: match generator-like styling defaults
            var baseText = TextStyle.Default
                .FontFamily("Tajawal")
                .FontSize(11);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(36);
                    page.DefaultTextStyle(baseText);

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        // TOP BANNER IMAGE
                        if (doc.BannerImageBytes is { Length: > 0 })
                        {
                            col.Item()
                                .Image(doc.BannerImageBytes)
                                .FitWidth();
                        }

                        // TITLE
                        col.Item().PaddingTop(6).AlignCenter().Text(text =>
                        {
                            text.DefaultTextStyle(baseText.FontSize(16).SemiBold());
                            text.Span("MEMO");
                        });

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // HEADER BLOCK (premium card)
                        col.Item().Element(e =>
                        {
                            e.Background(Colors.Grey.Lighten5)
                             .Border(1)
                             .BorderColor(Colors.Grey.Lighten2)
                             .Padding(12)
                             .CornerRadius(8)
                             .Column(h =>
                             {
                                 h.Spacing(6);

                                 HeaderRow(h, "To:", doc.To);
                                 if (!string.IsNullOrWhiteSpace(doc.Through))
                                     HeaderRow(h, "Through:", doc.Through);

                                 HeaderRow(h, "From:", doc.From);
                                 HeaderRow(h, "Subject:", doc.Subject);
                                 HeaderRow(h, "Classification:", doc.Classification);
                                 HeaderRow(h, "Date:", doc.Date.ToLocalTime().ToString("dd MMM yyyy"));
                                 HeaderRow(h, "Prepared by:", doc.PreparedBy);
                             });
                        });

                        // BODY
                        col.Item().PaddingTop(6).Element(e =>
                        {
                            e.Border(1)
                             .BorderColor(Colors.Grey.Lighten3)
                             .Padding(14)
                             .CornerRadius(10)
                             .Background(Colors.White)
                             .Column(body =>
                             {
                                 body.Spacing(6);

                                 var blocks = HtmlToTextBlocks(doc.BodyHtml);

                                 foreach (var b in blocks)
                                 {
                                     if (b.IsHeading)
                                     {
                                         body.Item().Text(t =>
                                         {
                                             t.DefaultTextStyle(baseText.FontSize(13).SemiBold());
                                             t.Span(b.Text);
                                         });
                                     }
                                     else
                                     {
                                         body.Item().Text(t =>
                                         {
                                             t.DefaultTextStyle(baseText.FontSize(11));
                                             t.Span(b.Text);
                                         });
                                     }
                                 }
                             });
                        });

                        // FOOTER IMAGE
                        if (doc.FooterImageBytes is { Length: > 0 })
                        {
                            col.Item()
                                .PaddingTop(10)
                                .Image(doc.FooterImageBytes)
                                .FitWidth();
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static void HeaderRow(ColumnDescriptor col, string label, string value)
        {
            col.Item().Row(r =>
            {
                r.ConstantItem(110).Text(t =>
                {
                    t.DefaultTextStyle(TextStyle.Default.FontFamily("Tajawal").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2));
                    t.Span(label);
                });

                r.RelativeItem().Text(t =>
                {
                    t.DefaultTextStyle(TextStyle.Default.FontFamily("Tajawal").FontSize(10));
                    t.Span(value ?? string.Empty);
                });
            });
        }

        private sealed record TextBlock(string Text, bool IsHeading);

        // Minimal HTML parsing (generator-style: turn html into readable blocks)
        private static TextBlock[] HtmlToTextBlocks(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return Array.Empty<TextBlock>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove images completely (matches your earlier strip)
            var imgs = doc.DocumentNode.SelectNodes("//img");
            if (imgs != null)
            {
                foreach (var img in imgs)
                    img.Remove();
            }

            // Treat headings as "premium" emphasis
            var nodes = doc.DocumentNode
                .Descendants()
                .Where(n => n.Name is "h1" or "h2" or "h3" or "p" or "li" or "br")
                .ToList();

            var blocks = new System.Collections.Generic.List<TextBlock>();

            foreach (var n in nodes)
            {
                var text = HtmlEntity.DeEntitize(n.InnerText ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var isHeading = n.Name is "h1" or "h2" or "h3";
                blocks.Add(new TextBlock(text, isHeading));
            }

            // fallback if html has no p/h nodes
            if (blocks.Count == 0)
            {
                var plain = HtmlEntity.DeEntitize(doc.DocumentNode.InnerText ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(plain))
                    blocks.Add(new TextBlock(plain, false));
            }

            return blocks.ToArray();
        }

        private byte[]? ReadWwwrootBytes(string relativePath)
        {
            var full = Path.Combine(_env.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            return File.Exists(full) ? File.ReadAllBytes(full) : null;
        }

        private static void EnsureFontsRegistered(IWebHostEnvironment env)
        {
            lock (_fontLock)
            {
                if (_fontsRegistered) return;

                var fontsDir = Path.Combine(env.WebRootPath, "fonts");

                // Register ALL Tajawal fonts you have (Regular/Bold/etc)
                // QuestPDF will use them when you call .FontFamily("Tajawal")
                if (Directory.Exists(fontsDir))
                {
                    foreach (var f in Directory.GetFiles(fontsDir, "Tajawal-*.ttf"))
                    {
                        FontManager.RegisterFont(File.OpenRead(f));
                    }
                }

                _fontsRegistered = true;
            }
        }
    }
}
