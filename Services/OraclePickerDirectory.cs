using System.Data;
using BADEAPORTAL.Data;
using BADEAPORTAL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BADEAPORTAL.Services
{
    public class OraclePickerDirectory : IPickerDirectory
    {
        private readonly PortalDbContext _db;
        public OraclePickerDirectory(PortalDbContext db) => _db = db;

        public async Task<IReadOnlyList<EmployeePickDto>> SearchEmployeesAsync(string? q, int take = 50, string lang = "en", CancellationToken ct = default)
        {
            var list = new List<EmployeePickDto>();
            var conn = _db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            // you said: no "fetch all" here
            if (string.IsNullOrWhiteSpace(q))
                return Array.Empty<EmployeePickDto>();

            var limit = take <= 0 ? 50 : Math.Min(take, 200);

            await using var cmd = conn.CreateCommand();
            var isArabic = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase);
            var nameCol = isArabic ? "NAME_ARABIC" : "NAME_ENG";

            // if you truly only want to search by name, keep ONLY the nameCol predicate
            cmd.CommandText = $@"
SELECT EMP_ID, {nameCol} AS NAME_COL, USERID
FROM (
    SELECT EMP_ID, {nameCol}, USERID
    FROM   BADEA_ADDONS.EMPLOYEES
    WHERE  EMP_ID IS NOT NULL
      AND  UPPER({nameCol}) LIKE :p_like
    ORDER BY {nameCol}
)
WHERE ROWNUM <= :p_take";


            cmd.Parameters.Clear();

            var pName = cmd.CreateParameter();
            pName.ParameterName = "p_name";
            pName.Value = q.Trim().ToUpperInvariant() + "%"; // STARTS WITH (normal)
            cmd.Parameters.Add(pName);

            var pTake = cmd.CreateParameter();
            pTake.ParameterName = "p_take";
            pTake.Value = limit;
            cmd.Parameters.Add(pTake);

            await using var rdr = await cmd.ExecuteReaderAsync(ct);
            while (await rdr.ReadAsync(ct))
            {
                // keep EXACTLY your existing mapping behavior
                var empId = rdr.GetString(0);                 // if this ever throws, EMP_ID is NUMBER and you must ToString it
                var name = rdr.IsDBNull(1) ? "-" : rdr.GetString(1);
                var userId = rdr.IsDBNull(2) ? null : rdr.GetString(2);

                list.Add(new EmployeePickDto
                {
                    EmpId = empId,
                    Label = $"{name} ({empId})",
                    UserId = userId
                });
            }

            return list;
        }

        public async Task<IReadOnlyList<EmployeePickDto>> ListEmployeesAsync(int take = 200, string lang = "en", CancellationToken ct = default)
        {
            var list = new List<EmployeePickDto>();
            var conn = _db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            var limit = take <= 0 ? 200 : Math.Min(take, 2000);

            await using var cmd = conn.CreateCommand();
            var isArabic = string.Equals(lang, "ar", StringComparison.OrdinalIgnoreCase);
            var nameCol = isArabic ? "NAME_ARABIC" : "NAME_ENG";

            cmd.CommandText = $@"
SELECT EMP_ID, {nameCol} AS NAME_COL, USERID
FROM (
    SELECT EMP_ID, {nameCol}, USERID
    FROM   BADEA_ADDONS.EMPLOYEES
    WHERE  EMP_ID IS NOT NULL
    ORDER  BY {nameCol}
)
WHERE ROWNUM <= :p_take";


            var pTake = cmd.CreateParameter();
            pTake.ParameterName = "p_take";
            pTake.Value = limit;
            cmd.Parameters.Add(pTake);

            await using var rdr = await cmd.ExecuteReaderAsync(ct);
            while (await rdr.ReadAsync(ct))
            {
                var empId = rdr.GetString(0);
                var name = rdr.IsDBNull(1) ? "-" : rdr.GetString(1);
                var userId = rdr.IsDBNull(2) ? null : rdr.GetString(2);

                list.Add(new EmployeePickDto
                {
                    EmpId = empId,
                    Label = $"{name} ({empId})",
                    UserId = userId
                });
            }

            return list;
        }



        public async Task<IReadOnlyList<DepartmentPickDto>> SearchDepartmentsAsync(string? q, int take = 20, string lang = "en", CancellationToken ct = default)
        {
            var list = new List<DepartmentPickDto>();
            var conn = _db.Database.GetDbConnection();

            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            var limit = take <= 0 ? 20 : Math.Min(take, 50);

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT *
FROM (
    SELECT DEPT_CODE, DEPT_NAME, HEAD_EMP_ID
    FROM   BADEA_ADDONS.DEPARTMENTS
    WHERE  :p_q IS NULL
       OR  UPPER(DEPT_NAME) LIKE :p_like
       OR  UPPER(DEPT_CODE) LIKE :p_like
    ORDER BY DEPT_NAME
)
WHERE ROWNUM <= :p_take";

            var pQ = cmd.CreateParameter();
            pQ.ParameterName = "p_q";
            pQ.Value = string.IsNullOrWhiteSpace(q) ? DBNull.Value : q.Trim();
            cmd.Parameters.Add(pQ);

            var pLike = cmd.CreateParameter();
            pLike.ParameterName = "p_like";
            // pLike.Value = string.IsNullOrWhiteSpace(q) ? DBNull.Value : $"%{q.Trim().ToUpperInvariant()}%";
            pLike.Value = $"{q.Trim().ToUpperInvariant()}%";

            cmd.Parameters.Add(pLike);

            var pTake = cmd.CreateParameter();
            pTake.ParameterName = "p_take";
            pTake.Value = limit;
            cmd.Parameters.Add(pTake);

            await using var rdr = await cmd.ExecuteReaderAsync(ct);
            while (await rdr.ReadAsync(ct))
            {
                var code = rdr.GetString(0);
                var name = rdr.IsDBNull(1) ? "-" : rdr.GetString(1);
                var head = rdr.IsDBNull(2) ? null : rdr.GetString(2);

                list.Add(new DepartmentPickDto
                {
                    DeptCode = code,
                    DeptName = name,
                    HeadEmpId = head,
                    Label = $"{name} ({code})"
                });
            }

            return list;
        }


        public async Task<(string EmpId, string NameEng)?> TryGetEmployeeByUserIdAsync(string userId, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(userId)) return null;

            static string NormalizeSam(string raw)
            {
                var s = raw.Trim();
                var bs = s.LastIndexOf('\\');
                if (bs >= 0 && bs < s.Length - 1) s = s[(bs + 1)..];
                var at = s.IndexOf('@');
                if (at > 0) s = s[..at];
                return s.Trim();
            }

            var sam = NormalizeSam(userId).ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(sam)) return null;

            var conn = _db.Database.GetDbConnection();
            if (conn.State != ConnectionState.Open)
                await conn.OpenAsync(ct);

            // exact match first
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT EMP_ID, NAME_ENG
FROM   BADEA_ADDONS.EMPLOYEES
WHERE  UPPER(USERID) = :p_exact";

                var p = cmd.CreateParameter();
                p.ParameterName = "p_exact";
                p.Value = sam;
                cmd.Parameters.Add(p);

                await using var r = await cmd.ExecuteReaderAsync(ct);
                if (await r.ReadAsync(ct))
                {
                    var emp = r.GetString(0);
                    var name = r.IsDBNull(1) ? "" : r.GetString(1);
                    return (emp, name);
                }
            }

            // fallback contains
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT EMP_ID, NAME_ENG
FROM   BADEA_ADDONS.EMPLOYEES
WHERE  UPPER(USERID) LIKE :p_like
   OR  UPPER(USERID) LIKE :p_domLike
   OR  UPPER(USERID) LIKE :p_mailLike
ORDER BY LENGTH(USERID) ASC";

                var p1 = cmd.CreateParameter(); p1.ParameterName = "p_like"; p1.Value = $"%{sam}%";
                var p2 = cmd.CreateParameter(); p2.ParameterName = "p_domLike"; p2.Value = $"%\\{sam}";
                var p3 = cmd.CreateParameter(); p3.ParameterName = "p_mailLike"; p3.Value = $"{sam}@%";
                cmd.Parameters.Add(p1); cmd.Parameters.Add(p2); cmd.Parameters.Add(p3);

                await using var r = await cmd.ExecuteReaderAsync(ct);
                if (await r.ReadAsync(ct))
                {
                    var emp = r.GetString(0);
                    var name = r.IsDBNull(1) ? "" : r.GetString(1);
                    return (emp, name);
                }
            }

            // last resort email contains
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT EMP_ID, NAME_ENG
FROM   BADEA_ADDONS.EMPLOYEES
WHERE  UPPER(EMAIL) LIKE :p_mailAny";

                var p = cmd.CreateParameter();
                p.ParameterName = "p_mailAny";
                p.Value = $"%{sam}%";
                cmd.Parameters.Add(p);

                await using var r = await cmd.ExecuteReaderAsync(ct);
                if (await r.ReadAsync(ct))
                {
                    var emp = r.GetString(0);
                    var name = r.IsDBNull(1) ? "" : r.GetString(1);
                    return (emp, name);
                }
            }

            return null;
        }
    }
}
