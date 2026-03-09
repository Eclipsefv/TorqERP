using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using TorqERP.Services;
using TorqERP.ViewModels;

using Uri = System.Uri;

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
            const string BaseUrl = "http://localhost:3000/";

            // 3.Autentication
            builder.Services.AddAuthorizationCore();

            //concrete auth custom class
            builder.Services.AddScoped<SimpleAuthStateProvider>();

            //base type referencing same class as above
            builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
                sp.GetRequiredService<SimpleAuthStateProvider>());

            //auth logic servuce
            builder.Services.AddHttpClient<AuthService>(client =>
            {
                client.BaseAddress = new Uri(BaseUrl);
            });

            //style
            builder.Services.AddSingleton<TorqThemeService>();

            //API Handling (bearers for token auth)
            builder.Services.AddTransient<TokenHandler>();
            builder.Services.AddHttpClient<ApiService>(client =>
            {
                client.BaseAddress = new Uri(BaseUrl);
            })
            .AddHttpMessageHandler<TokenHandler>();

            //VMs
            builder.Services.AddScoped<CustomersViewModel>();
            builder.Services.AddTransient<VehiclesViewModel>();
            builder.Services.AddTransient<UsersViewModel>();
            builder.Services.AddTransient<ProductsViewModel>();
            builder.Services.AddTransient<WorkOrdersViewModel>();
            builder.Services.AddTransient<AppointmentsViewModel>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}