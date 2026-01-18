using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BADEAPORTAL.Models
{
    [Table("DEPARTMENTS", Schema = "BADEA_ADDONS")]
    public class Department
    {
        [Key]
        [Column("DEPT_CODE")]
        [StringLength(30)]
        public string DeptCode { get; set; } = null!;

        [Column("DEPT_NAME")]
        [StringLength(200)]
        public string DeptName { get; set; } = null!;

        [Column("HEAD_EMP_ID")]
        [StringLength(50)]
        public string? HeadEmpId { get; set; }
    }
}
