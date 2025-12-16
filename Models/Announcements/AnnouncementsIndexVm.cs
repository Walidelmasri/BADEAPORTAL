using System.Collections.Generic;

namespace BADEAPORTAL.Models.Announcements
{
    public class AnnouncementsIndexVm
    {
        public List<AnnouncementSummaryVm> Items { get; set; } = new();
        public int Page { get; set; }
        public int TotalPages { get; set; }
    }
}
