using System.Text.Json;

namespace PlotThoseLines.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;

        public class AppSettings
        {
            public string? ApiKey { get; set; }
        }

        private AppSettings _settings = new();

        public SettingsService()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appFolder = Path.Combine(documentsPath, "PlotThoseLines");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "settings.json");

            Load();
        }

        public string? GetApiKey() => _settings.ApiKey ?? Environment.GetEnvironmentVariable("TI_API_KEY") ?? string.Empty;

        public async Task SetApiKeyAsync(string? apiKey)
        {
            _settings.ApiKey = apiKey?.Trim();
            await SaveAsync();
        }

        private void Load()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    _settings = new AppSettings();
                }
            }
        }

        private async Task SaveAsync()
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
    }
}
