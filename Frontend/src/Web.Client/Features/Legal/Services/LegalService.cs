using Web.Infrastructure.Http;

namespace Web.Features.Legal.Services;

public class LegalService
{
    private readonly VendorHttpClient _api;
    private readonly ILogger<LegalService> _logger;

    public LegalService(VendorHttpClient api, ILogger<LegalService> logger)
    {
        _api = api;
        _logger = logger;
    }

    public async Task<List<ContractItem>> GetContractsAsync(CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching contracts from API");
        try
        {
            var result = await _api.GetAsync<List<ContractItem>>("legal/contracts", ct);
            _logger.LogInformation("Fetched {Count} contracts", result.Count);
            return result;
        }
        catch (VendorApiException ex)
        {
            _logger.LogError(ex, "Failed to fetch contracts");
            throw;
        }
    }
}

public class ContractItem
{
    public long Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string? PartyName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
