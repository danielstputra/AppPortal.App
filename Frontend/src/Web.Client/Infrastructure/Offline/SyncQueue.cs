using System.Text.Json;

namespace Web.Infrastructure.Offline;

public class SyncQueue
{
    private readonly IndexedDbService _db;
    private const string QueueStore = "sync_queue";
    private const string MetadataStore = "sync_metadata";

    public SyncQueue(IndexedDbService db)
    {
        _db = db;
    }

    public async Task EnqueueAsync(string entityType, SyncAction action, object payload)
    {
        var item = new SyncQueueItem
        {
            Id = Guid.NewGuid().ToString(),
            EntityType = entityType,
            Action = action,
            PayloadJson = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow,
            Status = QueueStatus.Pending
        };
        await _db.UpsertAsync(QueueStore, item);
    }

    public async Task<SyncQueueItem?> DequeueAsync()
    {
        var items = await _db.GetAllAsync<SyncQueueItem>(QueueStore);
        return items.Where(i => i.Status == QueueStatus.Pending).OrderBy(i => i.CreatedAt).FirstOrDefault();
    }

    public async Task<List<SyncQueueItem>> GetAllPendingAsync()
    {
        var items = await _db.GetAllAsync<SyncQueueItem>(QueueStore);
        return items.Where(i => i.Status == QueueStatus.Pending).OrderBy(i => i.CreatedAt).ToList();
    }

    public async Task MarkCompletedAsync(string itemId)
    {
        await _db.DeleteAsync(QueueStore, itemId);
    }

    public async Task MarkFailedAsync(string itemId, string error)
    {
        var item = await _db.GetByIdAsync<SyncQueueItem>(QueueStore, itemId);
        if (item is null) return;
        item.RetryCount++;
        item.LastError = error;
        item.LastAttemptAt = DateTime.UtcNow;
        if (item.RetryCount >= 5) item.Status = QueueStatus.Conflict;
        await _db.UpsertAsync(QueueStore, item);
    }

    public async Task<int> GetPendingCountAsync()
    {
        var items = await _db.GetAllAsync<SyncQueueItem>(QueueStore);
        return items.Count(i => i.Status == QueueStatus.Pending);
    }

    public async Task UpdateLastSyncTimeAsync()
    {
        await _db.UpsertAsync(MetadataStore, new SyncMetadata { Id = "last_sync", LastSyncAt = DateTime.UtcNow });
    }

    public async Task<DateTime?> GetLastSyncTimeAsync()
    {
        var meta = await _db.GetByIdAsync<SyncMetadata>(MetadataStore, "last_sync");
        return meta?.LastSyncAt;
    }
}

public enum SyncAction { Create, Update, Delete }
public enum QueueStatus { Pending, Conflict }

public class SyncQueueItem
{
    public string Id { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public SyncAction Action { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public QueueStatus Status { get; set; } = QueueStatus.Pending;
    public int RetryCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? LastAttemptAt { get; set; }
}

public class SyncMetadata
{
    public string Id { get; set; } = "last_sync";
    public DateTime LastSyncAt { get; set; }
}
