using System;

namespace BADEAPORTAL.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string BodyHtml { get; set; } = null!;

        public bool IsMemo { get; set; }

        // Memo header fields (only used when IsMemo == true)
        public string? MemoTo { get; set; }
        public string? MemoThrough { get; set; }
        public string? MemoFrom { get; set; }
        public string? MemoSubject { get; set; }
        public string? MemoClassification { get; set; }

        // Audit
        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByName { get; set; } = null!;
        public string CreatedByUpn { get; set; } = null!;
    }
}
    