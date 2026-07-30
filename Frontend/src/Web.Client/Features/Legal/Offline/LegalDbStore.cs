using Web.Infrastructure.Offline;

namespace Web.Features.Legal.Offline;

/// <summary>
/// Store khusus module Legal ke IndexedDB.
/// </summary>
public class LegalDbStore
{
    private readonly IndexedDbService _db;
    private const string ContractsStore = "legal_contracts";

    public LegalDbStore(IndexedDbService db)
    {
        _db = db;
    }

    public async Task SaveContractsAsync(List<LegalContractItem> items)
    {
        await _db.UpsertBatchAsync(ContractsStore, items);
    }

    public async Task<List<LegalContractItem>> GetContractsAsync()
    {
        return await _db.GetAllAsync<LegalContractItem>(ContractsStore);
    }

    public async Task<LegalContractItem?> GetContractAsync(string id)
    {
        return await _db.GetByIdAsync<LegalContractItem>(ContractsStore, id);
    }

    public async Task ClearAsync()
    {
        await _db.ClearStoreAsync(ContractsStore);
    }
}

public class LegalContractItem
{
    public string Id { get; set; } = string.Empty;
    public string ContractNumber { get; set; } = string.Empty;
    public string? PartyName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}
