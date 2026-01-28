using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using TorqERP.Services;

namespace TorqERP
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

            // 1.MudBlazor
            builder.Services.AddMudServices();

            // 2.HttpClient
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("http://localhost:3000/")
            });

            // 3.Autentication
            builder.Services.AddAuthorizationCore();

            //concrete auth custom class
            builder.Services.AddScoped<SimpleAuthStateProvider>();

            //base type referencing same class as above
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<SimpleAuthStateProvider>());

            //auth logic servuce
            builder.Services.AddScoped<AuthService>();

            //style
            builder.Services.AddSingleton<TorqThemeService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}