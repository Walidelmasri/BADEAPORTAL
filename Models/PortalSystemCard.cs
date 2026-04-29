namespace BADEAPORTAL.Models
{
    public class PortalSystemCard
    {
        public int CardId { get; set; }
        public int? SysId { get; set; }

        public string SysNameEn { get; set; } = "";
        public string SysNameAr { get; set; } = "";

        public string? DescriptionEn { get; set; }
        public string? DescriptionAr { get; set; }

        public string? CategoryEn { get; set; }
        public string? CategoryAr { get; set; }

        public string AppUrl { get; set; } = "";
        public string? LogoPath { get; set; }

        public string? RoleGroup { get; set; }

        public bool IsPublic { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}