namespace BADEAPORTAL.ViewModels
{
    public class EmployeePickDto
    {
        public string EmpId { get; set; } = "";
        public string Label { get; set; } = ""; // "Name (EMP_ID)"
        public string? UserId { get; set; }
        public string? Email { get; set; }
    }
}
