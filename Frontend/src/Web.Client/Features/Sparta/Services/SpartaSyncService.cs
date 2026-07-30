using Web.Infrastructure.Http;
using Web.Infrastructure.Offline;
using Web.Features.Sparta.Offline;

namespace Web.Features.Sparta.Services;

public class SpartaSyncService
{
    private readonly SpartaService _onlineService;
    private readonly SpartaDbStore _localDb;
    private readonly SyncQueue _syncQueue;
    private readonly NetworkStatusService _network;
    private readonly ILogger<SpartaSyncService> _logger;

    public SpartaSyncService(
        SpartaService onlineService,
        SpartaDbStore localDb,
        SyncQueue syncQueue,
        NetworkStatusService network,
        ILogger<SpartaSyncService> logger)
    {
        _onlineService = onlineService;
        _localDb = localDb;
        _syncQueue = syncQueue;
        _network = network;
        _logger = logger;
    }

    public async Task SaveGradingAsync(GradingPayload payload)
    {
        if (_network.IsOnline)
        {
            try
            {
                var result = await _onlineService.SubmitGradingAsync(payload);
                await _localDb.SaveGradingItemAsync(new GradingItem
                {
                    Id = result.Id,
                    TruckPlate = payload.TruckPlate,
                    Weight = payload.Weight,
                    SyncedAt = DateTime.UtcNow
                });
                _logger.LogInformation("Grading saved online, cached locally");
                return;
            }
            catch (VendorApiException ex)
            {
                _logger.LogWarning(ex, "API save failed — falling back to offline");
            }
        }

        var localItem = new GradingItem
        {
            Id = DateTime.UtcNow.Ticks,
            TruckPlate = payload.TruckPlate,
            Weight = payload.Weight,
            CreatedAt = DateTime.UtcNow,
            IsPendingSync = true
        };

        await _localDb.SaveGradingItemAsync(localItem);
        await _syncQueue.EnqueueAsync("sparta_grading", SyncAction.Create, payload);
        _logger.LogInformation("Grading saved locally — queued for sync");
    }

    public async Task<List<GradingItem>> GetGradingListAsync()
    {
        if (_network.IsOnline)
        {
            try
            {
                var data = await _onlineService.GetGradingListAsync();
                await _localDb.SaveGradingListAsync(data);
                return data;
            }
            catch { }
        }
        var localData = await _localDb.GetGradingListAsync();
        if (localData.Count == 0)
            _logger.LogWarning("No data available — offline and no cache");
        return localData;
    }
}
