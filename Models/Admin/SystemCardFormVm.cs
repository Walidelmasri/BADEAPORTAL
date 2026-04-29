using Microsoft.AspNetCore.Http;

namespace BADEAPORTAL.Models.Admin
{
    public class SystemCardFormVm
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

        public int IsActive { get; set; } = 1;
        public int IsPublic { get; set; } = 1;

        public IFormFile? LogoFile { get; set; }
    }
}