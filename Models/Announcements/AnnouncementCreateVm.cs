using System.ComponentModel.DataAnnotations;

namespace BADEAPORTAL.Models.Announcements
{
    public class AnnouncementCreateVm
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string BodyHtml { get; set; } = null!; // Quill / rich-text HTML

        public bool IsMemo { get; set; }

        // Memo fields (only meaningful when IsMemo == true)
        public string? MemoTo { get; set; }
        public string? MemoThrough { get; set; }
        public string? MemoFrom { get; set; }
        public string? MemoSubject { get; set; }
        public string? MemoClassification { get; set; }
    }
}
