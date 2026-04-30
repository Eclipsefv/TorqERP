using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using System.Globalization;
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

            //MudBlazor
            builder.Services.AddMudServices();

            //HttpClient
            const string BaseUrl = "http://localhost:3000/";

            //Autentication
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
            builder.Services.AddTransient<InvoicesViewModel>();
            builder.Services.AddTransient<SuppliersViewModel>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif
            //set everything to euros
            var culture = new CultureInfo("es-ES");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            return builder.Build();
        }
    }
}