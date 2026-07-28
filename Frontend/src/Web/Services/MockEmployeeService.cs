using System.Text.Json;
using Web.Models;
using Web.Services.Http;

namespace Web.Services;

/// <summary>
/// Mock service that reads employee data from wwwroot/mock-data/employees.json via IApiClient.
/// Supports full CRUD operations with file persistence.
/// When the real API is ready, replace this with ApiEmployeeService (using the same IApiClient).
/// </summary>
public class MockEmployeeService : IEmployeeService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<MockEmployeeService> _logger;
    private readonly IWebHostEnvironment _env;
    private List<Employee>? _cachedEmployees;

    private const string Endpoint = "/employees";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public MockEmployeeService(IApiClient apiClient, ILogger<MockEmployeeService> logger, IWebHostEnvironment env)
    {
        _apiClient = apiClient;
        _logger = logger;
        _env = env;
    }

    private async Task<List<Employee>> GetEmployeesInternalAsync()
    {
        if (_cachedEmployees != null)
            return _cachedEmployees;

        _logger.LogInformation("Loading employee data from mock API: {Endpoint}", Endpoint);

        var response = await _apiClient.GetAsync<List<Employee>>(Endpoint);

        if (!response.IsSuccess || response.Data == null)
        {
            var errMsg = response.Error?.Message ?? "Unknown error";
            _logger.LogError("Failed to load employee data: {Message}", errMsg);
            return new List<Employee>();
        }

        _cachedEmployees = response.Data;
        _logger.LogInformation("Loaded {Count} employees from mock data", _cachedEmployees.Count);
        return _cachedEmployees;
    }

    private string GetFilePath()
    {
        return Path.Combine(_env.WebRootPath, "mock-data", "employees.json");
    }

    private async Task PersistAsync()
    {
        try
        {
            var filePath = GetFilePath();
            var json = JsonSerializer.Serialize(_cachedEmployees, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);
            _logger.LogDebug("Saved {Count} employees to {File}", _cachedEmployees?.Count ?? 0, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist employee data");
        }
    }

    public async Task<IEnumerable<Employee>> GetEmployeesAsync(string? searchTerm = null, string? unitKerja = null, StatusKaryawan? status = null)
    {
        var employees = await GetEmployeesInternalAsync();
        var query = employees.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLowerInvariant();
            query = query.Where(e =>
                e.NamaKaryawan.ToLowerInvariant().Contains(term) ||
                e.Nik.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(unitKerja))
        {
            query = query.Where(e =>
                e.UnitKerja.Contains(unitKerja, StringComparison.OrdinalIgnoreCase));
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        return query.ToList();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        var employees = await GetEmployeesInternalAsync();
        return employees.FirstOrDefault(e => e.No == id);
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        var employees = await GetEmployeesInternalAsync();

        employee.No = employees.Count > 0 ? employees.Max(e => e.No) + 1 : 1;
        employees.Add(employee);

        await PersistAsync();
        _logger.LogInformation("Created employee: {No} - {Name}", employee.No, employee.NamaKaryawan);
        return employee;
    }

    public async Task<Employee?> UpdateEmployeeAsync(int id, Employee employee)
    {
        var employees = await GetEmployeesInternalAsync();
        var existing = employees.FirstOrDefault(e => e.No == id);
        if (existing == null) return null;

        existing.Nik = employee.Nik;
        existing.NamaKaryawan = employee.NamaKaryawan;
        existing.Jabatan = employee.Jabatan;
        existing.UnitKerja = employee.UnitKerja;
        existing.Status = employee.Status;

        await PersistAsync();
        _logger.LogInformation("Updated employee: {No} - {Name}", id, employee.NamaKaryawan);
        return existing;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employees = await GetEmployeesInternalAsync();
        var removed = employees.RemoveAll(e => e.No == id);

        if (removed > 0)
            await PersistAsync();

        if (removed > 0)
            _logger.LogInformation("Deleted employee: {No}", id);
        return removed > 0;
    }

    public async Task<IEnumerable<string>> GetUnitKerjaListAsync()
    {
        var employees = await GetEmployeesInternalAsync();
        return employees
            .Select(e => e.UnitKerja)
            .Distinct()
            .OrderBy(u => u)
            .ToList();
    }

    public async Task<int> GetTotalCountAsync()
    {
        var employees = await GetEmployeesInternalAsync();
        return employees.Count;
    }
}
