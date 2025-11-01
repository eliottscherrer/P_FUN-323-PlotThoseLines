using Microsoft.AspNetCore.WebUtilities;
using PlotThoseLines.Extensions;
using System.Net.Http.Json;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PlotThoseLines.Services
{
    // Custom converter to handle invalid double values from API
    public class SafeDoubleConverter : JsonConverter<double>
    {
        public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return 0.0;
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();
                if (string.IsNullOrWhiteSpace(stringValue) || 
                    stringValue.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                    stringValue.Equals("nan", StringComparison.OrdinalIgnoreCase) ||
                    stringValue.Equals("infinity", StringComparison.OrdinalIgnoreCase))
                {
                    return 0.0;
                }

                if (double.TryParse(stringValue, out var result))
                {
                    return double.IsNaN(result) || double.IsInfinity(result) ? 0.0 : result;
                }

                return 0.0;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                var value = reader.GetDouble();
                return double.IsNaN(value) || double.IsInfinity(value) ? 0.0 : value;
            }

            // For any other type, return 0
            return 0.0;
        }

        public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
            {
                writer.WriteNumberValue(0.0);
            }
            else
            {
                writer.WriteNumberValue(value);
            }
        }
    }

    public class CoinHistoryService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public CoinHistoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new SafeDoubleConverter() },
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals
            };
        }

        public async Task<Dictionary<string, CoinHistoryDataResponse?>> FetchHistoryDataAsync(
            IEnumerable<LocalAsset> assets,
            string interval,
            int dataLength,
            string vsCurrency = "usd")
        {
            var result = new Dictionary<string, CoinHistoryDataResponse?>();

            var fetchTasks = assets.Select(async asset =>
            {
                if (string.IsNullOrWhiteSpace(asset.Id))
                    return (asset.Id, (CoinHistoryDataResponse?)null);

                try
                {
                    // Check if this is a local asset
                    if (asset.IsLocal)
                    {
                        if (asset.HistoryData == null || !asset.HistoryData.Any())
                        {
                            return (asset.Id!, (CoinHistoryDataResponse?)null);
                        }

                        // Convert local asset data to API format on a background thread
                        var localData = await Task.Run(() => ConvertLocalAssetToApiFormat(asset, interval, dataLength)).ConfigureAwait(false);
                        return (asset.Id!, localData);
                    }

                    // Fetch from API for non-local assets
                    var queryParams = new Dictionary<string, string?>
                    {
                        { "interval", interval },
                        { "length", dataLength.ToString() },
                        { "vs_currency", vsCurrency }
                    };
                    var url = QueryHelpers.AddQueryString($"history/coins/{asset.Id}", queryParams);
                    
                    // Use custom JSON options to handle invalid values
                    var httpResponse = await _httpClient.GetAsync(url).ConfigureAwait(false);
                    httpResponse.EnsureSuccessStatusCode();
                    
                    var stream = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var response = await JsonSerializer.DeserializeAsync<ApiResponse>(stream, _jsonOptions).ConfigureAwait(false);

                    if (response?.status?.code == 0 && response?.data != null)
                    {
                        return (asset.Id!, response.data);
                    }

                    return (asset.Id!, (CoinHistoryDataResponse?)null);
                }
                catch (HttpRequestException httpEx)
                {
                    Debug.WriteLine($"Network error fetching data for {asset.Symbol}: {httpEx.Message}");
                    return (asset.Id!, (CoinHistoryDataResponse?)null);
                }
                catch (JsonException jsonEx)
                {
                    Debug.WriteLine($"JSON parsing error for {asset.Symbol}: {jsonEx.Message}");
                    Debug.WriteLine($"This usually means the API returned invalid data. Creating placeholder data for {asset.Symbol}");
                    
                    // Create minimal placeholder data instead of returning null
                    return (asset.Id!, new CoinHistoryDataResponse
                    {
                        id = asset.Id,
                        name = asset.Name,
                        symbol = asset.Symbol,
                        logo = asset.Logo,
                        vs_currency = vsCurrency,
                        market_chart = new[]
                        {
                            new MarketChart
                            {
                                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                price = 0.01,
                                market_cap = 0.0,
                                vol_spot_24h = 0.0
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    return (asset.Id!, (CoinHistoryDataResponse?)null);
                }
            });

            var results = await Task.WhenAll(fetchTasks).ConfigureAwait(false);

            return results
                .Where(r => !string.IsNullOrEmpty(r.Item1) && r.Item2 != null)
                .ToDictionary(r => r.Item1!, r => r.Item2);
        }

        private CoinHistoryDataResponse ConvertLocalAssetToApiFormat(LocalAsset asset, string interval, int dataLength)
        {
            var emptyResponse = new CoinHistoryDataResponse
            {
                id = asset.Id,
                name = asset.Name,
                symbol = asset.Symbol,
                logo = asset.Logo,
                market_chart = Array.Empty<MarketChart>(),
                vs_currency = "usd"
            };

            if (asset.HistoryData == null || !asset.HistoryData.Any())
            {
                Debug.WriteLine($"No history data available for {asset.Symbol}");
                return emptyResponse;
            }

            try
            {
                Debug.WriteLine($"Converting {asset.HistoryData.Count} local data points for {asset.Symbol}...");

                var marketChartData = asset.HistoryData
                    .Select(data =>
                    {
                        try
                        {
                            if (!data.Price.HasValue || data.Price.Value <= 0 || string.IsNullOrWhiteSpace(data.Date))
                            {
                                return null;
                            }

                            var dateString = data.Date.Replace("UTC+0", "").Trim();
                            if (DateTime.TryParse(dateString, System.Globalization.CultureInfo.InvariantCulture,
                                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                                out var parsedDate))
                            {
                                return new MarketChart
                                {
                                    timestamp = new DateTimeOffset(parsedDate, TimeSpan.Zero).ToUnixTimeMilliseconds(),
                                    price = data.Price.Value,
                                    market_cap = data.Market_cap ?? 0.0,
                                    vol_spot_24h = data.Volume ?? 0.0
                                };
                            }
                            else
                            {
                                Debug.WriteLine($"Failed to parse date '{data.Date}' for {asset.Symbol}, skipping entry");
                                return null;
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error processing data point for {asset.Symbol}: {ex.Message}");
                            return null;
                        }
                    })
                    .Where(mc => mc != null)
                    .Select(mc => mc!)
                    .ToList();

                if (!marketChartData.Any())
                {
                    // Create a single placeholder data point to avoid empty dataset
                    // This allows the chart to display something rather than failing completely
                    var now = DateTimeOffset.UtcNow;
                    marketChartData.Add(new MarketChart
                    {
                        timestamp = now.ToUnixTimeMilliseconds(),
                        price = 0.01,
                        market_cap = 0.0,
                        vol_spot_24h = 0.0
                    });
                }

                var sortedData = marketChartData.OrderByDescending(m => m.timestamp).ToArray();

               MarketChart[] filteredData;
                if (dataLength > 0 && sortedData.Length > dataLength)
                {
                    filteredData = sortedData.Take(dataLength).OrderBy(m => m.timestamp).ToArray();
                }
                else
                {
                    filteredData = sortedData.OrderBy(m => m.timestamp).ToArray();
                }

                return new CoinHistoryDataResponse
                {
                    id = asset.Id,
                    name = asset.Name,
                    symbol = asset.Symbol,
                    logo = asset.Logo,
                    market_chart = filteredData,
                    vs_currency = "usd"
                };
            }
            catch (Exception ex)
            {
                return emptyResponse;
            }
        }

        public List<EnhancedMarketChart> EnhanceChartData(CoinHistoryDataResponse? data)
        {
            if (data?.market_chart == null || !data.market_chart.Any())
            {
                return new List<EnhancedMarketChart>();
            }

            try
            {
                var rng = new Random();
                var points = data.market_chart.Where(p => p.price > 0).ToArray();
                
                if (!points.Any())
                {
                    return new List<EnhancedMarketChart>();
                }

                double previousClose = 0;
                const double anchorWeight = 0.4;
                const double noisePct = 0.015;
                const double maxJumpPct = 0.05;
                const double minWickPct = 0.001;
                const double maxWickPct = 0.02;

                var result = points.Select(entry =>
                {
                    try
                    {
                        var basePrice = entry.price;

                        if (basePrice <= 0 || double.IsNaN(basePrice) || double.IsInfinity(basePrice))
                        {
                            basePrice = previousClose > 0 ? previousClose : 1.0; // Fallback to previous or 1.0
                        }

                        double open = previousClose > 0 ? previousClose : basePrice;

                        double blended = anchorWeight * basePrice + (1 - anchorWeight) * open;
                        double noise = (rng.NextDouble() - 0.5) * 2 * noisePct * blended;
                        double closeUnclamped = blended + noise;

                        double minClose = open * (1 - maxJumpPct);
                        double maxClose = open * (1 + maxJumpPct);
                        double close = Math.Clamp(closeUnclamped, minClose, maxClose);

                        double bodyPct = Math.Abs(close - open) / open;
                        double wickPct = bodyPct * 0.6 + 0.003 + (rng.NextDouble() - 0.5) * 0.003;
                        wickPct = Math.Clamp(wickPct, minWickPct, maxWickPct);

                        double high = Math.Max(open, close) * (1.0 + wickPct);
                        double low = Math.Min(open, close) * (1.0 - wickPct);

                        previousClose = close;

                        return new EnhancedMarketChart
                        {
                            DateTime = entry.timestamp.ToLocalDateTimeFromUnixMs(),
                            Price = basePrice,
                            Open = Math.Round(open, 2),
                            Close = Math.Round(close, 2),
                            Low = Math.Round(low, 2),
                            High = Math.Round(high, 2),
                            MarketCap = double.IsNaN(entry.market_cap) || double.IsInfinity(entry.market_cap) ? 0.0 : entry.market_cap,
                            Volume = double.IsNaN(entry.vol_spot_24h) || double.IsInfinity(entry.vol_spot_24h) ? 0.0 : entry.vol_spot_24h
                        };
                    }
                    catch (Exception)
                    {
                        try
                        {
                            return new EnhancedMarketChart
                            {
                                DateTime = entry.timestamp.ToLocalDateTimeFromUnixMs(),
                                Price = previousClose > 0 ? previousClose : 1.0,
                                Open = previousClose > 0 ? previousClose : 1.0,
                                Close = previousClose > 0 ? previousClose : 1.0,
                                Low = previousClose > 0 ? previousClose * 0.99 : 0.99,
                                High = previousClose > 0 ? previousClose * 1.01 : 1.01,
                                MarketCap = 0.0,
                                Volume = 0.0
                            };
                        }
                        catch
                        {
                            return null;
                        }
                    }
                })
                .Where(x => x != null)
                .Select(x => x!)
                .ToList();

                if (!result.Any())
                {
                    Debug.WriteLine("No enhanced data points created, returning minimal placeholder");
                    result.Add(new EnhancedMarketChart
                    {
                        DateTime = DateTime.UtcNow,
                        Price = 1.0,
                        Open = 1.0,
                        Close = 1.0,
                        Low = 0.99,
                        High = 1.01,
                        MarketCap = 0.0,
                        Volume = 0.0
                    });
                }
                
                return result;
            }
            catch (Exception ex)
            {
                return new List<EnhancedMarketChart>
                {
                    new EnhancedMarketChart
                    {
                        DateTime = DateTime.UtcNow,
                        Price = 1.0,
                        Open = 1.0,
                        Close = 1.0,
                        Low = 0.99,
                        High = 1.01,
                        MarketCap = 0.0,
                        Volume = 0.0
                    }
                };
            }
        }
    }

    // DTOs
    public class ApiResponse
    {
        public CoinHistoryDataResponse? data { get; set; }
        public Status? status { get; set; }
    }

    public class CoinHistoryDataResponse
    {
        public string? id { get; set; }
        public string? logo { get; set; }
        public MarketChart[]? market_chart { get; set; }
        public string? name { get; set; }
        public string? symbol { get; set; }
        public string? vs_currency { get; set; }
        public PageInfo? page_info { get; set; }
    }

    public class MarketChart
    {
        [JsonConverter(typeof(SafeDoubleConverter))]
        public double market_cap { get; set; }
        
        [JsonConverter(typeof(SafeDoubleConverter))]
        public double price { get; set; }
        
        public long timestamp { get; set; }
        
        [JsonConverter(typeof(SafeDoubleConverter))]
        public double vol_spot_24h { get; set; }
    }

    public class EnhancedMarketChart
    {
        public DateTime DateTime { get; set; }
        public double Price { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Low { get; set; }
        public double High { get; set; }
        public double MarketCap { get; set; }
        public double Volume { get; set; }
    }

    public class PageInfo
    {
        public int total_results { get; set; }
    }

    public class Status
    {
        public int code { get; set; }
        public string? message { get; set; }
        public long timestamp { get; set; }
    }
}
