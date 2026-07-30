using Web.Infrastructure.Http;

namespace Web.Features.Sparta.Services;

public class SpartaService
{
    private readonly VendorHttpClient _api;
    private readonly ILogger<SpartaService> _logger;

    public SpartaService(VendorHttpClient api, ILogger<SpartaService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<List<GradingItem>> GetGradingListAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching grading list from API");
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            var result = await _api.GetAsync<List<GradingItem>>("sparta/grading", ct);
            sw.Stop();
            _logger.LogInformation("Fetched {Count} grading items in {Elapsed}ms", result.Count, sw.ElapsedMilliseconds);
            return result;
        }
        catch (VendorApiException ex)
        {
            _logger.LogError(ex, "Failed to fetch grading list after {Elapsed}ms", sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<GradingResult> SubmitGradingAsync(GradingPayload payload, CancellationToken ct = default)
    {
        _logger.LogDebug("Submitting grading payload");
        var result = await _api.PostAsync<GradingPayload, GradingResult>("sparta/grading", payload, ct);
        _logger.LogInformation("Grading submitted successfully");
        return result;
    }
}

public class GradingItem
{
    public long Id { get; set; }
    public string TruckPlate { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string? Grade { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SyncedAt { get; set; }
    public bool IsPendingSync { get; set; }
}

public class GradingPayload
{
    public string TruckPlate { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string? Notes { get; set; }
}

public class GradingResult
{
    public long Id { get; set; }
    public bool Success { get; set; }
    public string? Message { get; set; }
}
