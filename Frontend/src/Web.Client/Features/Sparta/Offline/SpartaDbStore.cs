using Web.Infrastructure.Offline;
using Web.Features.Sparta.Services;

namespace Web.Features.Sparta.Offline;

public class SpartaDbStore
{
    private readonly IndexedDbService _db;
    private const string GradingStore = "sparta_grading";
    private const string MasterDataStore = "sparta_masterdata";

    public SpartaDbStore(IndexedDbService db)
    {
        _db = db;
    }

    public async Task SaveGradingListAsync(List<GradingItem> items, CancellationToken ct = default)
    {
        await _db.UpsertBatchAsync(GradingStore, items);
    }

    public async Task<List<GradingItem>> GetGradingListAsync(CancellationToken ct = default)
    {
        return await _db.GetAllAsync<GradingItem>(GradingStore);
    }

    public async Task SaveGradingItemAsync(GradingItem item)
    {
        await _db.UpsertAsync(GradingStore, item);
    }

    public async Task<GradingItem?> GetGradingItemAsync(long id)
    {
        return await _db.GetByIdAsync<GradingItem>(GradingStore, id);
    }

    public async Task ClearAllAsync()
    {
        await _db.ClearStoreAsync(GradingStore);
        await _db.ClearStoreAsync(MasterDataStore);
    }
}
