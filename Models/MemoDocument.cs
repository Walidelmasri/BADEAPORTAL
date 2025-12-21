using System;

namespace BADEAPORTAL.Models
{
    public sealed class MemoDocument
    {
        public string MemoNumber { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;
        public string Through { get; set; } = string.Empty;
        public string From { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Classification { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public string PreparedBy { get; set; } = string.Empty;

        public string BodyHtml { get; set; } = string.Empty;

        public byte[]? BannerImageBytes { get; set; }
        public byte[]? FooterImageBytes { get; set; }
    }
}
