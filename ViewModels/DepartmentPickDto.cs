namespace BADEAPORTAL.ViewModels
{
    public class DepartmentPickDto
    {
        public string DeptCode { get; set; } = "";
        public string Label { get; set; } = ""; // "Dept Name (Code)"
        public string DeptName { get; set; } = "";
        public string? HeadEmpId { get; set; }
    }
}
