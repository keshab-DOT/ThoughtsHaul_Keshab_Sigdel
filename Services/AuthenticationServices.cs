namespace MauiApp1.Services;

public class AuthService
{
    // 🔐 DEFAULT SINGLE USER CREDENTIALS
    private const string DefaultUsername = "admin";
    private const string DefaultPassword = "1234";

    public bool IsLoggedIn { get; private set; }

    public string? CurrentUser { get; private set; }

    public Task<bool> Login(string username, string password)
    {
        if (username == DefaultUsername && password == DefaultPassword)
        {
            IsLoggedIn = true;
            CurrentUser = username;
            return Task.FromResult(true);
        }

        IsLoggedIn = false;
        return Task.FromResult(false);
    }

    public void Logout()
    {
        IsLoggedIn = false;
        CurrentUser = null;
    }
}
