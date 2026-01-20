using BADEAPORTAL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BADEAPORTAL.Controllers
{
    [Authorize]
    [Route("api/lookup")]
    public class LookupController : Controller
    {
        private readonly IPickerDirectory _pickers;
        public LookupController(IPickerDirectory pickers) => _pickers = pickers;

        [HttpGet("employees")]
        public async Task<IActionResult> Employees([FromQuery] string? q, [FromQuery] int take = 20, [FromQuery] string lang = "en", CancellationToken ct = default)
        {
            var results = await _pickers.SearchEmployeesAsync(q, take, lang, ct);
            return Ok(results);
        }

        [HttpGet("employees/list")]
        public async Task<IActionResult> EmployeesList([FromQuery] int take = 200, [FromQuery] string lang = "en", CancellationToken ct = default)
        {
            var results = await _pickers.ListEmployeesAsync(take, lang, ct);
            return Ok(results);
        }

        [HttpGet("departments")]
        public async Task<IActionResult> Departments([FromQuery] string? q, [FromQuery] int take = 20, [FromQuery] string lang = "en", CancellationToken ct = default)
        {
            var results = await _pickers.SearchDepartmentsAsync(q, take, lang, ct);
            return Ok(results);
        }


    }
}
