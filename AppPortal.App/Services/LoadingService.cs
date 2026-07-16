namespace AppPortal.App.Services;

public sealed class LoadingService
{
    private int _activeCount;

    public bool IsLoading => _activeCount > 0;

    public string? Message { get; private set; }

    public event Action? OnChange;

    public void Show(string? message = null)
    {
        Interlocked.Increment(ref _activeCount);

        Message = message;

        OnChange?.Invoke();
    }

    public void Hide()
    {
        if (_activeCount > 0)
            Interlocked.Decrement(ref _activeCount);

        if (_activeCount == 0)
            Message = null;

        OnChange?.Invoke();
    }

    public async Task RunAsync(Func<Task> action, string? message = null)
    {
        Show(message);

        try
        {
            await action();
        }
        finally
        {
            Hide();
        }
    }

    public async Task<T> RunAsync<T>(Func<Task<T>> action, string? message = null)
    {
        Show(message);

        try
        {
            return await action();
        }
        finally
        {
            Hide();
        }
    }
}