using System;

namespace BADEAPORTAL.Models.Announcements
{
    public class AnnouncementDetailsVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string BodyHtml { get; set; } = null!;

        public bool IsMemo { get; set; }
        public string? MemoTo { get; set; }
        public string? MemoThrough { get; set; }
        public string? MemoFrom { get; set; }
        public string? MemoSubject { get; set; }
        public string? MemoClassification { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByName { get; set; } = null!;
    }
}
