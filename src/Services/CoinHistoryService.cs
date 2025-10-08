using Microsoft.AspNetCore.WebUtilities;
using PlotThoseLines.Extensions;
using System.Net.Http.Json;

namespace PlotThoseLines.Services
{
    public class CoinHistoryService
    {
        private readonly HttpClient _httpClient;

        public CoinHistoryService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
                    if (asset.IsLocal && asset.HistoryData != null && asset.HistoryData.Any())
                    {
                        // Convert local asset data to API format on a background thread
                        var localData = await Task.Run(() => ConvertLocalAssetToApiFormat(asset));
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
                    var response = await _httpClient.GetFromJsonAsync<ApiResponse>(url);

                    if (response?.status?.code == 0 && response?.data != null)
                    {
                        return (asset.Id!, response.data);
                    }

                    return (asset.Id!, (CoinHistoryDataResponse?)null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching data for {asset.Symbol}: {ex.Message}");
                    return (asset.Id!, (CoinHistoryDataResponse?)null);
                }
            });

            var results = await Task.WhenAll(fetchTasks);

            foreach (var (assetId, data) in results)
            {
                if (!string.IsNullOrEmpty(assetId))
                {
                    result[assetId] = data;
                }
            }

            return result;
        }

        private CoinHistoryDataResponse ConvertLocalAssetToApiFormat(LocalAsset asset)
        {
            if (asset.HistoryData == null || !asset.HistoryData.Any())
            {
                return new CoinHistoryDataResponse
                {
                    id = asset.Id,
                    name = asset.Name,
                    symbol = asset.Symbol,
                    logo = asset.Logo,
                    market_chart = Array.Empty<MarketChart>()
                };
            }

            try
            {
                var marketChartData = asset.HistoryData
                    .AsParallel() // Use parallel processing for large datasets
                    .Where(data => data.Price.HasValue) // Filter out entries with null prices
                    .Select(data =>
                    {
                        // Parse the date string to Unix timestamp
                        long timestamp = 0;
                        try
                        {
                            if (!string.IsNullOrEmpty(data.Date))
                            {
                                // Try to parse the date more efficiently
                                if (DateTime.TryParse(data.Date, out var parsedDate))
                                {
                                    timestamp = new DateTimeOffset(parsedDate).ToUnixTimeMilliseconds();
                                }
                                else
                                {
                                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                }
                            }
                            else
                            {
                                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing date '{data.Date}': {ex.Message}");
                            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        }

                        return new MarketChart
                        {
                            timestamp = timestamp,
                            price = data.Price.GetValueOrDefault(0.0),
                            market_cap = data.Market_cap.GetValueOrDefault(0.0),
                            vol_spot_24h = data.Volume.GetValueOrDefault(0.0)
                        };
                    })
                    .OrderBy(m => m.timestamp)
                    .ToArray();

                Console.WriteLine($"Converted {marketChartData.Length} data points for local asset {asset.Symbol}");

                return new CoinHistoryDataResponse
                {
                    id = asset.Id,
                    name = asset.Name,
                    symbol = asset.Symbol,
                    logo = asset.Logo,
                    market_chart = marketChartData,
                    vs_currency = "usd"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting local asset data for {asset.Symbol}: {ex.Message}");
                return new CoinHistoryDataResponse
                {
                    id = asset.Id,
                    name = asset.Name,
                    symbol = asset.Symbol,
                    logo = asset.Logo,
                    market_chart = Array.Empty<MarketChart>(),
                    vs_currency = "usd"
                };
            }
        }

        public List<EnhancedMarketChart> EnhanceChartData(CoinHistoryDataResponse? data)
        {
            if (data?.market_chart == null || !data.market_chart.Any())
                return new List<EnhancedMarketChart>();

            var rng = new Random();
            var points = data.market_chart.ToArray();
            var result = new List<EnhancedMarketChart>(points.Length);
            double previousClose = points.First().price;

            // Constants for data enhancement
            const double anchorWeight = 0.7; // Make the "Close" stay not too different from the base price
            const double maxJumpPct = 0.25;
            const double noisePct = 0.003;
            const double minWickPct = 0.002;
            const double maxWickPct = 0.06;

            result = points.Select(entry =>
            {
                var basePrice = entry.price;
                double open = previousClose;

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
                    MarketCap = entry.market_cap,
                    Volume = entry.vol_spot_24h
                };
            }).ToList();

            return result;
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
        public double market_cap { get; set; }
        public double price { get; set; }
        public long timestamp { get; set; }
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
