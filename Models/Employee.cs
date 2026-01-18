using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BADEAPORTAL.Models
{
    [Table("EMPLOYEES", Schema = "BADEA_ADDONS")]
    public class Employee
    {
        [Key]
        [Column("EMP_ID")]
        [StringLength(5)]
        public string EmpId { get; set; } = null!;

        [Column("NAME_ENG")]
        [StringLength(255)]
        public string? NameEng { get; set; }

        [Column("USERID")]
        [StringLength(50)]
        public string? UserId { get; set; }

        [Column("EMAIL")]
        [StringLength(50)]
        public string? Email { get; set; }

        [Column("DEPT_CODE")]
        [StringLength(3)]
        public string? DeptCode { get; set; }
    }
}
