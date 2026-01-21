namespace BADEAPORTAL.Models
{
    public class AnnouncementCreateDto
    {
        public string Title { get; set; } = null!;
        public string BodyHtml { get; set; } = null!;

        public bool IsMemo { get; set; }

        public string? MemoTo { get; set; }
        public string? MemoThrough { get; set; }
        public string? MemoFrom { get; set; }
        public string? MemoSubject { get; set; }
        public string? MemoClassification { get; set; }
        public string FromKind { get; set; } = "USER";
        public string? FromDeptCode { get; set; }

        public bool NotifyInApp { get; set; } = true;
        public bool NotifyEmail { get; set; } = true;
        public string ToKind { get; set; } = "USER";
        public string? ToDeptCode { get; set; }

    }
}
