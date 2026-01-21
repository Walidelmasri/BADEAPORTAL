using System.ComponentModel.DataAnnotations;

namespace BADEAPORTAL.Models.Announcements
{
    public class AnnouncementCreateVm
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string BodyHtml { get; set; } = null!;

        public bool IsMemo { get; set; }

        public string? MemoTo { get; set; }
        public string? MemoThrough { get; set; }
        public string? MemoFrom { get; set; }
        public string? MemoSubject { get; set; }
        public string? MemoClassification { get; set; }
        public string FromKind { get; set; } = "USER";   // USER or DEPT
        public string? FromDeptCode { get; set; }

        public bool NotifyInApp { get; set; } = true;
        public bool NotifyEmail { get; set; } = true;
        public string ToKind { get; set; } = "USER";
        public string? ToDeptCode { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (!IsMemo)
                yield break;

            if (string.IsNullOrWhiteSpace(MemoTo))
                yield return new ValidationResult("Required", new[] { nameof(MemoTo) });

            if (string.IsNullOrWhiteSpace(MemoFrom))
                yield return new ValidationResult("Required", new[] { nameof(MemoFrom) });

            if (string.IsNullOrWhiteSpace(MemoSubject))
                yield return new ValidationResult("Required", new[] { nameof(MemoSubject) });

            if (string.IsNullOrWhiteSpace(MemoClassification))
                yield return new ValidationResult("Required", new[] { nameof(MemoClassification) });
        }
    }
}
