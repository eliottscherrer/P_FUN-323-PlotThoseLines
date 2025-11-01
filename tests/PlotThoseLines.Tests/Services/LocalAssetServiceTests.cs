using PlotThoseLines.Services;

namespace PlotThoseLines.Tests.Services
{
    public class LocalAssetServiceTests
    {
        [Fact]
        public void GetMinimumDataInterval_WithHourlyIntervals_ReturnsHour()
        {
            // Arrange
            var service = new LocalAssetService();
            var baseDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            const string TEST_ASSET_ID = "test";
            const double INITIAL_PRICE = 100.0;
            const int HOUR_INTERVAL = 1;
            
            var asset = new LocalAsset
            {
                Id = TEST_ASSET_ID,
                HistoryData = new List<LocalAssetHistoryData>
                {
                    new LocalAssetHistoryData { Date = baseDate.ToString("o"), Price = INITIAL_PRICE },
                    new LocalAssetHistoryData { Date = baseDate.AddHours(HOUR_INTERVAL).ToString("o"), Price = INITIAL_PRICE + 1.0 },
                    new LocalAssetHistoryData { Date = baseDate.AddHours(HOUR_INTERVAL * 2).ToString("o"), Price = INITIAL_PRICE + 2.0 },
                }
            };

            // Act
            var result = service.GetMinimumDataInterval(asset);

            // Assert
            const string EXPECTED_INTERVAL = "hour";
            Assert.Equal(EXPECTED_INTERVAL, result);
        }

        [Fact]
        public void GetMinimumDataInterval_WithDailyIntervals_ReturnsDay()
        {
            // Arrange
            var service = new LocalAssetService();
            var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            const string TEST_ASSET_ID = "test";
            const double INITIAL_PRICE = 100.0;
            const int DAY_INTERVAL = 1;
            
            var asset = new LocalAsset
            {
                Id = TEST_ASSET_ID,
                HistoryData = new List<LocalAssetHistoryData>
                {
                    new LocalAssetHistoryData { Date = baseDate.ToString("o"), Price = INITIAL_PRICE },
                    new LocalAssetHistoryData { Date = baseDate.AddDays(DAY_INTERVAL).ToString("o"), Price = INITIAL_PRICE + 1.0 },
                    new LocalAssetHistoryData { Date = baseDate.AddDays(DAY_INTERVAL * 2).ToString("o"), Price = INITIAL_PRICE + 2.0 },
                }
            };

            // Act
            var result = service.GetMinimumDataInterval(asset);

            // Assert
            const string EXPECTED_INTERVAL = "day";
            Assert.Equal(EXPECTED_INTERVAL, result);
        }
    }
}
