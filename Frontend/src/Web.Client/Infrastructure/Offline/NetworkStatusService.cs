using Microsoft.JSInterop;

namespace Web.Infrastructure.Offline;

public class NetworkStatusService : IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private readonly ILogger<NetworkStatusService> _logger;
    private DotNetObjectReference<NetworkStatusService>? _dotNetRef;

    public event Action<bool>? OnStatusChanged;
    public bool IsOnline { get; private set; } = true;

    public NetworkStatusService(IJSRuntime js, ILogger<NetworkStatusService> logger)
    {
        _js = js;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        IsOnline = await _js.InvokeAsync<bool>("eval", "navigator.onLine");
        await _js.InvokeVoidAsync("networkStatusHelper.listen", _dotNetRef, "OnNetworkChange");
        _logger.LogInformation("Network status initialized: {Status}", IsOnline ? "Online" : "Offline");
    }

    [JSInvokable]
    public void OnNetworkChange(bool isOnline)
    {
        if (IsOnline == isOnline) return;
        IsOnline = isOnline;
        _logger.LogWarning("Network status changed to: {Status}", isOnline ? "Online" : "Offline");
        OnStatusChanged?.Invoke(isOnline);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        if (_js is not null)
        {
            try { await _js.InvokeVoidAsync("networkStatusHelper.dispose"); }
            catch { }
        }
    }
}
