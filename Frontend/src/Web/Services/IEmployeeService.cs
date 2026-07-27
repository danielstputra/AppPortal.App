using Web.Models;

namespace Web.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetEmployeesAsync(string? searchTerm = null, string? unitKerja = null, StatusKaryawan? status = null);
    Task<IEnumerable<string>> GetUnitKerjaListAsync();
    Task<int> GetTotalCountAsync();
}
