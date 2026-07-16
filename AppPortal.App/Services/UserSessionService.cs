namespace AppPortal.App.Services;

public sealed class UserSessionService
{
    public Guid Id { get; private set; }

    public string UserName { get; private set; } = "";

    public string FullName { get; private set; } = "";

    public string Email { get; private set; } = "";

    public string Role { get; private set; } = "";

    public bool IsAuthenticated { get; private set; }

    public event Action? OnChange;

    public void SignIn(Guid id,
                       string username,
                       string fullname,
                       string email,
                       string role)
    {
        Id = id;
        UserName = username;
        FullName = fullname;
        Email = email;
        Role = role;

        IsAuthenticated = true;

        OnChange?.Invoke();
    }

    public void SignOut()
    {
        Id = Guid.Empty;
        UserName = "";
        FullName = "";
        Email = "";
        Role = "";

        IsAuthenticated = false;

        OnChange?.Invoke();
    }
}