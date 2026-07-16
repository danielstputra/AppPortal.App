namespace AppPortal.App.Services;

public enum AppTheme
{
    Light,
    Dark
}

public sealed class ThemeService
{
    public AppTheme CurrentTheme { get; private set; }
        = AppTheme.Light;

    public event Action? OnChange;

    public void Toggle()
    {
        CurrentTheme =
            CurrentTheme == AppTheme.Light
            ? AppTheme.Dark
            : AppTheme.Light;

        OnChange?.Invoke();
    }

    public void SetTheme(AppTheme theme)
    {
        CurrentTheme = theme;

        OnChange?.Invoke();
    }

    public string CssClass =>
        CurrentTheme == AppTheme.Dark
            ? "theme-dark"
            : "theme-light";
}