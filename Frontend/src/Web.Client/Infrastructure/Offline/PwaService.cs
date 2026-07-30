using Microsoft.JSInterop;

namespace Web.Infrastructure.Offline;

public class PwaService
{
    private readonly IJSRuntime _js;
    private const string SwPath = "service-worker.js";

    public PwaService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task RegisterServiceWorkerAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("navigator.serviceWorker.register", SwPath);
            Console.WriteLine("[PWA] Service Worker registered");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[PWA] Failed to register SW: {ex.Message}");
        }
    }

    public async Task<bool> CheckForUpdateAsync()
    {
        var registration = await _js.InvokeAsync<IJSObjectReference>(
            "navigator.serviceWorker.getRegistration");
        if (registration is null) return false;
        await registration.InvokeVoidAsync("update");
        return true;
    }
}
