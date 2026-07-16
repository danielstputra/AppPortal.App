namespace AppPortal.App.Services;

public class BreadcrumbItem
{
    public string Text { get; set; } = "";

    public string Url { get; set; } = "";
}

public sealed class BreadcrumbService
{
    public List<BreadcrumbItem> Items { get; }
        = new();

    public event Action? OnChange;

    public void Set(params BreadcrumbItem[] items)
    {
        Items.Clear();

        Items.AddRange(items);

        OnChange?.Invoke();
    }

    public void Clear()
    {
        Items.Clear();

        OnChange?.Invoke();
    }
}