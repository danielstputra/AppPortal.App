using Web.Infrastructure.Http;

namespace Web.Infrastructure.Offline;

public class ConflictResolver
{
    private readonly ILogger<ConflictResolver> _logger;

    public ConflictResolver(ILogger<ConflictResolver> logger)
    {
        _logger = logger;
    }

    public ConflictResolution Resolve(SyncQueueItem item, VendorApiException apiError)
    {
        if (apiError.StatusCode == 409)
        {
            _logger.LogWarning("Conflict detected for {Entity} ({Id}) — applying Last-Write-Wins", item.EntityType, item.Id);
            return ConflictResolution.RetryWithForce;
        }
        return ConflictResolution.MarkForReview;
    }
}

public enum ConflictResolution
{
    RetryWithForce,
    AcceptServer,
    MarkForReview
}
