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

            string apiKey = Environment.GetEnvironmentVariable("TI_API_KEY") ?? "";

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://api.tokeninsight.com/api/v1/"),
                DefaultRequestHeaders = { { "TI_API_KEY", apiKey } }
            });

            builder.Services.AddSingleton<LocalAssetService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
