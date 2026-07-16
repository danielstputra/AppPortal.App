namespace AppPortal.App.Services;

public sealed class NavigationService
{
    public string CurrentPage { get; private set; } = "";

    public event Action? OnChange;

    public void SetPage(string page)
    {
        CurrentPage = page;

        OnChange?.Invoke();
    }
}