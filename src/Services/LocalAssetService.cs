using System.Text.Json;

namespace PlotThoseLines.Services
{
    public class LocalAssetService
    {
        private readonly string _assetsFilePath;
        private List<LocalAsset> _assets = new();

        public LocalAssetService()
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var appFolder = Path.Combine(documentsPath, "PlotThoseLines");
            Directory.CreateDirectory(appFolder);
            _assetsFilePath = Path.Combine(appFolder, "local_assets.json");
            
            LoadAssets();
        }

        public List<LocalAsset> GetAssets()
        {
            return _assets.ToList();
        }

        public async Task<bool> AddAssetAsync(LocalAsset asset)
        {
            // Check if asset already exists
            if (_assets.Any(a => a.Id?.Equals(asset.Id, StringComparison.OrdinalIgnoreCase) == true))
            {
                return false;
            }

            _assets.Add(asset);
            await SaveAssetsAsync();
            return true;
        }

        public async Task<bool> RemoveAssetAsync(string assetId)
        {
            var assetToRemove = _assets.FirstOrDefault(a => a.Id?.Equals(assetId, StringComparison.OrdinalIgnoreCase) == true);

            if (assetToRemove != null)
            {
                _assets.Remove(assetToRemove);
                await SaveAssetsAsync();
                return true;
            }

            return false;
        }

        private void LoadAssets()
        {
            if (File.Exists(_assetsFilePath))
            {
                var json = File.ReadAllText(_assetsFilePath);
                var assets = JsonSerializer.Deserialize<List<LocalAsset>>(json);
                _assets = assets ?? new List<LocalAsset>();
            }
        }

        private async Task SaveAssetsAsync()
        {
            var json = JsonSerializer.Serialize(_assets, new JsonSerializerOptions { WriteIndented = true });

            await File.WriteAllTextAsync(_assetsFilePath, json);
        }
    }

    public class LocalAsset
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Symbol { get; set; }
        public string? Logo { get; set; }
        public double Price { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Now;
        public string? Url { get; set; }
    }
}