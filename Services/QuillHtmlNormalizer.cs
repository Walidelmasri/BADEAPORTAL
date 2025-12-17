using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace BADEAPORTAL.Services
{
    public sealed class QuillHtmlNormalizer : IHtmlContentNormalizer
    {
        private static readonly Regex ArabicRegex =
            new Regex(@"[\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF]", RegexOptions.Compiled);

        public string Normalize(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return html ?? string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            NormalizeBlocks(doc);
            NormalizeTables(doc);

            return doc.DocumentNode.InnerHtml;
        }

        private static void NormalizeBlocks(HtmlDocument doc)
        {
            foreach (var node in doc.DocumentNode.Descendants()
                     .Where(n => n.NodeType == HtmlNodeType.Element))
            {
                var classAttr = node.GetAttributeValue("class", "");

                bool isRtl =
                    classAttr.Contains("ql-direction-rtl") ||
                    ArabicRegex.IsMatch(node.InnerText);

                if (isRtl)
                {
                    node.SetAttributeValue("dir", "rtl");
                    ApplyStyle(node, "text-align:right");
                }

                if (classAttr.Contains("ql-align-center"))
                    ApplyStyle(node, "text-align:center");

                if (classAttr.Contains("ql-align-right"))
                    ApplyStyle(node, "text-align:right");

                if (classAttr.Contains("ql-align-justify"))
                    ApplyStyle(node, "text-align:justify");

                node.Attributes.Remove("class");
            }
        }

        private static void NormalizeTables(HtmlDocument doc)
        {
            foreach (var table in doc.DocumentNode.SelectNodes("//table") ?? Enumerable.Empty<HtmlNode>())
            {
                ApplyStyle(table, "border-collapse:collapse;width:100%");

                foreach (var cell in table.SelectNodes(".//td|.//th") ?? Enumerable.Empty<HtmlNode>())
                {
                    ApplyStyle(cell, "border:1px solid #e5e7eb;padding:6px;vertical-align:top");

                    if (ArabicRegex.IsMatch(cell.InnerText))
                        cell.SetAttributeValue("dir", "rtl");
                }
            }
        }

        private static void ApplyStyle(HtmlNode node, string style)
        {
            var existing = node.GetAttributeValue("style", "");
            if (!existing.Contains(style))
            {
                node.SetAttributeValue(
                    "style",
                    string.IsNullOrWhiteSpace(existing) ? style : $"{existing};{style}"
                );
            }
        }
    }
}
