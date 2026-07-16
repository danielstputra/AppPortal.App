namespace AppPortal.App.Services;

public enum NotificationType
{
    Success,
    Warning,
    Error,
    Info
}

public sealed class NotificationService
{
    public string Message { get; private set; } = "";

    public NotificationType Type { get; private set; }

    public bool Visible { get; private set; }

    public event Action? OnChange;

    public void Show(string message,
                     NotificationType type = NotificationType.Info)
    {
        Message = message;
        Type = type;
        Visible = true;

        OnChange?.Invoke();
    }

    public void Hide()
    {
        Visible = false;

        OnChange?.Invoke();
    }
}