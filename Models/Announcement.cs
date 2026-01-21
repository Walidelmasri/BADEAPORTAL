using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BADEAPORTAL.Models
{
    [Table("ANNOUNCEMENTS", Schema = "BADEA_ADDONS")]
    public class Announcement
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("TITLE")]
        [StringLength(200)]
        public string Title { get; set; } = null!;

        [Column("BODYHTML")]
        public string? BodyHtml { get; set; }

        [Column("ISMEMO")]
        public bool IsMemo { get; set; } // maps NUMBER(1,0)

        [Column("MEMOTO")]
        [StringLength(200)]
        public string? MemoTo { get; set; }

        [Column("MEMOTHROUGH")]
        [StringLength(200)]
        public string? MemoThrough { get; set; }

        [Column("MEMOFROM")]
        [StringLength(200)]
        public string? MemoFrom { get; set; }

        [Column("MEMOSUBJECT")]
        [StringLength(200)]
        public string? MemoSubject { get; set; }

        [Column("MEMOCLASSIFICATION")]
        [StringLength(100)]
        public string? MemoClassification { get; set; }

        [Column("CREATEDATUTC")]
        public DateTime CreatedAtUtc { get; set; }

        [Column("CREATEDBYNAME")]
        [StringLength(200)]
        public string CreatedByName { get; set; } = null!;

        [Column("CREATEDBYUPN")]
        [StringLength(256)]
        public string CreatedByUpn { get; set; } = null!;

        [Column("SCOPE_CODE")]
        [StringLength(20)]
        public string ScopeCode { get; set; } = "ALL";

        // approval later
        [Column("REQUIRES_APPROVAL")]
        public bool RequiresApproval { get; set; }

        [Column("APPROVAL_STATUS")]
        [StringLength(20)]
        public string ApprovalStatus { get; set; } = "APPROVED";

        [Column("APPROVED_AT_UTC")]
        public DateTime? ApprovedAtUtc { get; set; }

        [Column("APPROVED_BY_EMPID")]
        [StringLength(5)]
        public string? ApprovedByEmpId { get; set; }

        [Column("REJECTED_AT_UTC")]
        public DateTime? RejectedAtUtc { get; set; }

        [Column("REJECTED_BY_EMPID")]
        [StringLength(5)]
        public string? RejectedByEmpId { get; set; }

        [Column("REJECTION_REASON")]
        [StringLength(4000)]
        public string? RejectionReason { get; set; }

        [Column("NOTIFY_INAPP")]
        public bool NotifyInApp { get; set; }

        [Column("NOTIFY_EMAIL")]
        public bool NotifyEmail { get; set; }

        // sender selection (2B)
        [Column("FROM_KIND")]
        [StringLength(10)]
        public string FromKind { get; set; } = "USER"; // USER or DEPT

        [Column("FROM_DEPT_CODE")]
        [StringLength(3)]
        public string? FromDeptCode { get; set; }
        // recipient selection (To)
        [Column("TO_KIND")]
        [StringLength(10)]
        public string ToKind { get; set; } = "USER"; // USER or DEPT

        [Column("TO_DEPT_CODE")]
        [StringLength(30)]
        public string? ToDeptCode { get; set; }

    }
}
