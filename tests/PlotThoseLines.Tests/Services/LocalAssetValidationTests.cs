using PlotThoseLines.Services;

namespace PlotThoseLines.Tests.Services
{
    public class LocalAssetValidationTests
    {
        [Fact]
        public void LocalAsset_WithValidId_IsValid()
        {
            // Arrange
            var asset = new LocalAsset
            {
                Id = "bitcoin",
                Symbol = "BTC",
                Name = "Bitcoin"
            };

            // Act & Assert
            Assert.NotNull(asset.Id);
            Assert.False(string.IsNullOrWhiteSpace(asset.Id));
        }

        [Fact]
        public void LocalAsset_WithHistoryData_CanAccessData()
        {
            // Arrange
            var asset = new LocalAsset
            {
                Id = "bitcoin",
                HistoryData = new List<LocalAssetHistoryData>
                {
                    new LocalAssetHistoryData { Date = "2024-01-01", Price = 42000 }
                }
            };

            // Act & Assert
            Assert.NotNull(asset.HistoryData);
            Assert.Single(asset.HistoryData);
            Assert.Equal(42000, asset.HistoryData[0].Price);
        }
    }
}
