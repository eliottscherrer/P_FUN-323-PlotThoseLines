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
            var asset = new LocalAsset
            {
                Id = "test",
                HistoryData = new List<LocalAssetHistoryData>
                {
                    new LocalAssetHistoryData { Date = baseDate.ToString("o"), Price = 100 },
                    new LocalAssetHistoryData { Date = baseDate.AddHours(1).ToString("o"), Price = 101 },
                    new LocalAssetHistoryData { Date = baseDate.AddHours(2).ToString("o"), Price = 102 },
                }
            };

            // Act
            var result = service.GetMinimumDataInterval(asset);

            // Assert
            Assert.Equal("hour", result);
        }

        [Fact]
        public void GetMinimumDataInterval_WithDailyIntervals_ReturnsDay()
        {
            // Arrange
            var service = new LocalAssetService();
            var baseDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var asset = new LocalAsset
            {
                Id = "test",
                HistoryData = new List<LocalAssetHistoryData>
                {
                    new LocalAssetHistoryData { Date = baseDate.ToString("o"), Price = 100 },
                    new LocalAssetHistoryData { Date = baseDate.AddDays(1).ToString("o"), Price = 101 },
                    new LocalAssetHistoryData { Date = baseDate.AddDays(2).ToString("o"), Price = 102 },
                }
            };

            // Act
            var result = service.GetMinimumDataInterval(asset);

            // Assert
            Assert.Equal("day", result);
        }
    }
}
