using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using MauiApp1.Data;
using MauiApp1.Services;

namespace MauiApp1
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<DBContext>();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<JournalService>();
            builder.Services.AddSingleton<DBContext>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<AppState>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
