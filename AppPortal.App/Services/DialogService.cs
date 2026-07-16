using Microsoft.AspNetCore.Components;

namespace AppPortal.App.Services;

public sealed class DialogService
{
    public bool Visible { get; private set; }

    public string Title { get; private set; } = "";

    public RenderFragment? Content { get; private set; }

    public event Action? OnChange;

    public void Show(string title, RenderFragment content)
    {
        Title = title;
        Content = content;
        Visible = true;

        OnChange?.Invoke();
    }

    public void Close()
    {
        Visible = false;
        Content = null;

        OnChange?.Invoke();
    }
}