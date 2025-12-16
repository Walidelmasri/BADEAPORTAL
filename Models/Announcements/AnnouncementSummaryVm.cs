using System;

namespace BADEAPORTAL.Models
{
    public class AnnouncementSummaryVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Excerpt { get; set; } = null!;
        public bool IsMemo { get; set; }

        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByName { get; set; } = null!;
    }
}
