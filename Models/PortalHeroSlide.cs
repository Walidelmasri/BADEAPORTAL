namespace BADEAPORTAL.Models
{
    public class PortalHeroSlide
    {
        public int SlideId { get; set; }
        public string ImagePath { get; set; } = "";
        public string? AltTextEn { get; set; }
        public string? AltTextAr { get; set; }
        public int SortOrder { get; set; }
        public int IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}