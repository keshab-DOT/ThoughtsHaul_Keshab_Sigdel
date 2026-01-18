namespace MauiApp1.Services;

public class AppState
{
    public bool IsDarkMode { get; set; } // Global variable
    public string SearchQuery { get; set; } = "";

    public event Action OnChange;

    public void ToggleDarkMode()
    {
        IsDarkMode = !IsDarkMode;
        NotifyStateChanged();
    }

    public void NotifyStateChanged() => OnChange?.Invoke();
}