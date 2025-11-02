using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace PlotThoseLines.Services
{
    public class NullableDoubleConverter : JsonConverter<double?>
    {
        public override double? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue) || 
                    stringValue.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                    stringValue.Equals("nan", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (double.TryParse(stringValue, out var result))
                {
                    return result;
                }

                return null;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDouble();
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, double? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }

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

        public List<LocalAsset> GetApiAssets()
        {
            return _assets.Where(a => a.IsLocal == false).ToList();
        }

        public List<LocalAsset> GetLocalAssets()
        {
            return _assets.Where(a => a.IsLocal == true).ToList();
        }

        public string GetMinimumDataInterval(LocalAsset asset)
        {
            if (asset?.HistoryData == null || asset.HistoryData.Count < 2)
            {
                return "day"; // Default to day if we can't determine
            }

            try
            {
                var dates = asset.HistoryData
                    .Where(d => !string.IsNullOrWhiteSpace(d.Date))
                    .Select(d =>
                    {
                        var dateString = d.Date!.Replace("UTC+0", "").Trim();
                        if (DateTime.TryParse(dateString, System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                            out var parsedDate))
                        {
                            return parsedDate;
                        }
                        return (DateTime?)null;
                    })
                    .Where(d => d.HasValue)
                    .Select(d => d!.Value)
                    .OrderBy(d => d)
                    .Take(10)
                    .ToList();

                if (dates.Count < 2)
                {
                    return "day";
                }

                // Calculate the minimum interval between consecutive dates
                var intervals = new List<TimeSpan>();
                for (int i = 1; i < dates.Count; i++)
                {
                    intervals.Add(dates[i] - dates[i - 1]);
                }

                var minInterval = intervals.Min();

                // Determine the interval type
                if (minInterval.TotalMinutes < 2)
                {
                    return "minute";
                }
                else if (minInterval.TotalHours < 2)
                {
                    return "hour";
                }
                else
                {
                    return "day";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error detecting data interval for {asset.Symbol}: {ex.Message}");
                return "day";
            }
        }

        public async Task<bool> AddAssetAsync(LocalAsset asset)
        {
            try
            {
                if (asset == null || string.IsNullOrWhiteSpace(asset.Id))
                    return false;

                // Check if asset already exists
                if (_assets.Any(a => a.Id?.Equals(asset.Id, StringComparison.OrdinalIgnoreCase) == true))
                    return false;

                _assets.Add(asset);
                await SaveAssetsAsync();
                return true;
            }
            catch
            {
                return false;
            }
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
            try
            {
                if (File.Exists(_assetsFilePath))
                {
                    var json = File.ReadAllText(_assetsFilePath);
                    
                    if (string.IsNullOrWhiteSpace(json))
                    {
                        _assets = new List<LocalAsset>();
                        return;
                    }

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        ReadCommentHandling = JsonCommentHandling.Skip,
                        AllowTrailingCommas = true
                    };

                    var assets = JsonSerializer.Deserialize<List<LocalAsset>>(json, options);
                    _assets = assets ?? new List<LocalAsset>();
                    
                    Debug.WriteLine($"Loaded {_assets.Count} assets from file");
                    
                    // Validate assets
                    var invalidAssets = _assets.Where(a => string.IsNullOrWhiteSpace(a.Id)).ToList();
                }
                else
                {
                    _assets = new List<LocalAsset>();
                }
            }
            catch
            {
                _assets = new List<LocalAsset>();
            }
        }

        private async Task SaveAssetsAsync()
        {
            try
            {
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
                };

                var json = JsonSerializer.Serialize(_assets, options);
                await File.WriteAllTextAsync(_assetsFilePath, json);
            }
            catch
            {
                throw; // Re-throw to notify caller of save failure
            }
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
        public bool IsLocal { get; set; } = false;
        public List<LocalAssetHistoryData>? HistoryData { get; set; }
    }

    public class LocalAssetHistoryData
    {
        [JsonPropertyName("Date")]
        public string? Date { get; set; }
        
        [JsonPropertyName("Price")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? Price { get; set; }
        
        [JsonPropertyName("Volume")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? Volume { get; set; }
        
        [JsonPropertyName("Market_cap")]
        [JsonConverter(typeof(NullableDoubleConverter))]
        public double? Market_cap { get; set; }
    }
}