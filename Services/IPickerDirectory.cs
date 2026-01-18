using BADEAPORTAL.ViewModels;

namespace BADEAPORTAL.Services
{
    public interface IPickerDirectory
    {
        Task<IReadOnlyList<EmployeePickDto>> SearchEmployeesAsync(string? q, int take = 20, CancellationToken ct = default);
        Task<IReadOnlyList<DepartmentPickDto>> SearchDepartmentsAsync(string? q, int take = 20, CancellationToken ct = default);

        Task<(string EmpId, string NameEng)?> TryGetEmployeeByUserIdAsync(string userId, CancellationToken ct = default);
        Task<IReadOnlyList<EmployeePickDto>> ListEmployeesAsync(int take = 200, CancellationToken ct = default);

    }
}
