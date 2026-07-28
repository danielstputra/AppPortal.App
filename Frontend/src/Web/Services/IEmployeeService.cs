using Web.Models;

namespace Web.Services;

public interface IEmployeeService
{
    Task<IEnumerable<Employee>> GetEmployeesAsync(string? searchTerm = null, string? unitKerja = null, StatusKaryawan? status = null);
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Employee> CreateEmployeeAsync(Employee employee);
    Task<Employee?> UpdateEmployeeAsync(int id, Employee employee);
    Task<bool> DeleteEmployeeAsync(int id);
    Task<IEnumerable<string>> GetUnitKerjaListAsync();
    Task<int> GetTotalCountAsync();
}
