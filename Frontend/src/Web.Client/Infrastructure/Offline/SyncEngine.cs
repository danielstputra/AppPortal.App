using System.Text.Json;
using Web.Infrastructure.Http;

namespace Web.Infrastructure.Offline;

public class SyncEngine
{
    private readonly SyncQueue _queue;
    private readonly VendorHttpClient _api;
    private readonly NetworkStatusService _network;
    private readonly ILogger<SyncEngine> _logger;

    public event Action<int>? OnSyncCompleted;
    public event Action<string>? OnSyncError;
    public event Action<int>? OnSyncProgress;

    private bool _isProcessing;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public SyncEngine(SyncQueue queue, VendorHttpClient api, NetworkStatusService network, ILogger<SyncEngine> logger)
    {
        _queue = queue;
        _api = api;
        _network = network;
        _logger = logger;
        _network.OnStatusChanged += async (isOnline) => { if (isOnline) await ProcessQueueAsync(); };
    }

    public async Task ProcessQueueAsync()
    {
        if (_isProcessing || !_network.IsOnline) return;
        await _semaphore.WaitAsync();
        _isProcessing = true;
        try
        {
            var pendingItems = await _queue.GetAllPendingAsync();
            if (pendingItems.Count == 0) return;

            _logger.LogInformation("Starting sync for {Count} items", pendingItems.Count);
            var successCount = 0;

            for (int i = 0; i < pendingItems.Count; i++)
            {
                var item = pendingItems[i];
                OnSyncProgress?.Invoke(i + 1);
                try
                {
                    await ProcessItemAsync(item);
                    await _queue.MarkCompletedAsync(item.Id);
                    successCount++;
                }
                catch (VendorApiException ex) when (ex.StatusCode == 409)
                {
                    await _queue.MarkFailedAsync(item.Id, $"Conflict: {ex.Message}");
                    OnSyncError?.Invoke($"Konflik data pada {item.EntityType}");
                }
                catch (Exception ex)
                {
                    await _queue.MarkFailedAsync(item.Id, ex.Message);
                    _logger.LogError(ex, "Failed to sync item {ItemId}", item.Id);
                }
            }
            await _queue.UpdateLastSyncTimeAsync();
            OnSyncCompleted?.Invoke(successCount);
            _logger.LogInformation("Sync completed: {Success}/{Total}", successCount, pendingItems.Count);
        }
        finally { _isProcessing = false; _semaphore.Release(); }
    }

    private async Task ProcessItemAsync(SyncQueueItem item)
    {
        var endpoint = MapEntityToEndpoint(item.EntityType);
        var payload = JsonDocument.Parse(item.PayloadJson);
        switch (item.Action)
        {
            case SyncAction.Create:
                await _api.PostAsync<JsonElement, JsonElement>(endpoint, payload.RootElement);
                break;
            case SyncAction.Update:
                await _api.PostAsync<JsonElement, JsonElement>($"{endpoint}/update", payload.RootElement);
                break;
        }
    }

    private static string MapEntityToEndpoint(string entityType) => entityType switch
    {
        "sparta_grading" => "sparta/grading",
        "sparta_masterdata" => "sparta/master-data",
        "legal_contracts" => "legal/contracts",
        _ => entityType.Replace('_', '/')
    };
}
