using Web.Models;
using Web.Services.Http;

namespace Web.Services;

/// <summary>
/// Mock service that reads employee data from wwwroot/mock-data/employees.json via IApiClient.
/// When the real API is ready, replace this with ApiEmployeeService (using the same IApiClient).
/// </summary>
public class MockEmployeeService : IEmployeeService
{
    private readonly IApiClient _apiClient;
    private readonly ILogger<MockEmployeeService> _logger;
    private List<Employee>? _cachedEmployees;

    private const string Endpoint = "/employees";

    public MockEmployeeService(IApiClient apiClient, ILogger<MockEmployeeService> logger)
    {
        _apiClient = apiClient;
        _logger = logger;
    }

    private async Task<List<Employee>> GetEmployeesInternalAsync()
    {
        // Cache after first load
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
