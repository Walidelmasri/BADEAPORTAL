using System;
using System.Collections.Generic;

namespace BADEAPORTAL.Models.Home
{
    public sealed class HomeIndexVm
    {
        public List<AnnouncementPreviewVm> LatestAnnouncements { get; set; } = new();
        public List<PortalSystemCardVm> SystemCards { get; set; } = new();
        public List<PortalHeroSlideVm> HeroSlides { get; set; } = new();
    }
    public sealed class PortalHeroSlideVm
    {
        public int SlideId { get; set; }
        public string ImagePath { get; set; } = "";
        public string? AltTextEn { get; set; }
        public string? AltTextAr { get; set; }
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

    public sealed class PortalSystemCardVm
    {
        public int CardId { get; set; }

        public string SysNameEn { get; set; } = "";
        public string SysNameAr { get; set; } = "";

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        public string? CategoryEn { get; set; }
        public string? CategoryAr { get; set; }

        public string AppUrl { get; set; } = "";
        public string? LogoPath { get; set; }
    }
}