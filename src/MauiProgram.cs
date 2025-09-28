using ApexCharts;
using Microsoft.Extensions.Logging;
using PlotThoseLines.Services;

namespace PlotThoseLines
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            // Load environment variables from .env file (located in the build directory)
            DotNetEnv.Env.Load();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Charting library
            builder.Services.AddApexChartsMaui();

            // Services
            builder.Services.AddSingleton<LocalAssetService>();
            builder.Services.AddSingleton<SettingsService>();

            // HttpClient with API key header (from saved settings or environment var)
            builder.Services.AddScoped(sp =>
            {
                var settings = sp.GetRequiredService<SettingsService>();
                var apiKey = settings.GetApiKey();

                var client = new HttpClient { BaseAddress = new Uri("https://api.tokeninsight.com/api/v1/") };
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    client.DefaultRequestHeaders.Remove("TI_API_KEY");
                    client.DefaultRequestHeaders.Add("TI_API_KEY", apiKey);
                }
                return client;
            });

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
