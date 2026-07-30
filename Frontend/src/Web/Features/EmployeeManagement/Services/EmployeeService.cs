using System.Text.Json;
using Web.Infrastructure.Http;
using Web.Features.EmployeeManagement.Models;

namespace Web.Features.EmployeeManagement.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IWebHostEnvironment _env;
    private List<Employee>? _cachedEmployees;
    private const string Endpoint = "/employees";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public EmployeeService(IApiClient apiClient, ILogger<EmployeeService> logger, IWebHostEnvironment env)
    {
        _apiClient = apiClient; _logger = logger; _env = env;
    }

    private async Task<List<Employee>> GetEmployeesInternalAsync()
    {
        if (_cachedEmployees != null) return _cachedEmployees;
        var response = await _apiClient.GetAsync<List<Employee>>(Endpoint);
        if (!response.IsSuccess || response.Data == null) return new();
        _cachedEmployees = response.Data;
        return _cachedEmployees;
    }

    private string GetFilePath() => Path.Combine(_env.WebRootPath, "mock-data", "employees.json");

    private async Task PersistAsync()
    {
        try { var json = JsonSerializer.Serialize(_cachedEmployees, JsonOptions); await File.WriteAllTextAsync(GetFilePath(), json); }
        catch (Exception ex) { _logger.LogError(ex, "Persist failed"); }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesAsync(string? searchTerm = null, string? unitKerja = null, StatusKaryawan? status = null)
    {
        var employees = await GetEmployeesInternalAsync();
        var query = employees.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(searchTerm)) { var t = searchTerm.ToLowerInvariant(); query = query.Where(e => e.NamaKaryawan.ToLowerInvariant().Contains(t) || e.Nik.Contains(t)); }
        if (!string.IsNullOrWhiteSpace(unitKerja)) query = query.Where(e => e.UnitKerja.Contains(unitKerja, StringComparison.OrdinalIgnoreCase));
        if (status.HasValue) query = query.Where(e => e.Status == status.Value);
        return query.ToList();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id) { var all = await GetEmployeesInternalAsync(); return all.FirstOrDefault(e => e.No == id); }
    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        var employees = await GetEmployeesInternalAsync();
        employee.No = employees.Count > 0 ? employees.Max(e => e.No) + 1 : 1;
        employees.Add(employee); await PersistAsync(); return employee;
    }
    public async Task<Employee?> UpdateEmployeeAsync(int id, Employee employee)
    {
        var employees = await GetEmployeesInternalAsync();
        var existing = employees.FirstOrDefault(e => e.No == id);
        if (existing == null) return null;
        existing.Nik = employee.Nik; existing.NamaKaryawan = employee.NamaKaryawan; existing.Jabatan = employee.Jabatan;
        existing.UnitKerja = employee.UnitKerja; existing.Status = employee.Status; await PersistAsync(); return existing;
    }
    public async Task<bool> DeleteEmployeeAsync(int id) { var employees = await GetEmployeesInternalAsync(); var r = employees.RemoveAll(e => e.No == id); if (r > 0) await PersistAsync(); return r > 0; }
    public async Task<IEnumerable<string>> GetUnitKerjaListAsync() { var employees = await GetEmployeesInternalAsync(); return employees.Select(e => e.UnitKerja).Distinct().OrderBy(u => u).ToList(); }
    public async Task<int> GetTotalCountAsync() { var employees = await GetEmployeesInternalAsync(); return employees.Count; }
}
