using System;
using System.Collections.Generic;

namespace BADEAPORTAL.Models.Home
{
    public sealed class HomeIndexVm
    {
        public List<AnnouncementPreviewVm> LatestAnnouncements { get; set; } = new();
    }

    public sealed class AnnouncementPreviewVm
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Excerpt { get; set; } = "";
        public bool IsMemo { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public string CreatedByName { get; set; } = "";
    }
}
