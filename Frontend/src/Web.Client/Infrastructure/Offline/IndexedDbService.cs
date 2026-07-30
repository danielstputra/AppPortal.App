using System.Text.Json;
using Microsoft.JSInterop;

namespace Web.Infrastructure.Offline;

public class IndexedDbService
{
    private readonly IJSRuntime _js;
    private const string DbName = "AppPortalDB";
    private const int DbVersion = 1;

    public IndexedDbService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        await _js.InvokeVoidAsync("indexedDbHelper.openDatabase", DbName, DbVersion);
    }

    public async Task UpsertAsync<T>(string storeName, T item)
    {
        var json = JsonSerializer.Serialize(item);
        await _js.InvokeVoidAsync("indexedDbHelper.upsert", DbName, storeName, json);
    }

    public async Task UpsertBatchAsync<T>(string storeName, IEnumerable<T> items)
    {
        var jsonArray = JsonSerializer.Serialize(items);
        await _js.InvokeVoidAsync("indexedDbHelper.upsertBatch", DbName, storeName, jsonArray);
    }

    public async Task<List<T>> GetAllAsync<T>(string storeName)
    {
        var json = await _js.InvokeAsync<string>("indexedDbHelper.getAll", DbName, storeName);
        if (string.IsNullOrEmpty(json)) return new List<T>();
        return JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
    }

    public async Task<T?> GetByIdAsync<T>(string storeName, object id)
    {
        var json = await _js.InvokeAsync<string?>("indexedDbHelper.getById", DbName, storeName, id);
        if (string.IsNullOrEmpty(json)) return default;
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task DeleteAsync(string storeName, object id)
    {
        await _js.InvokeVoidAsync("indexedDbHelper.delete", DbName, storeName, id);
    }

    public async Task ClearStoreAsync(string storeName)
    {
        await _js.InvokeVoidAsync("indexedDbHelper.clearStore", DbName, storeName);
    }

    public async Task<int> CountAsync(string storeName)
    {
        return await _js.InvokeAsync<int>("indexedDbHelper.count", DbName, storeName);
    }
}
