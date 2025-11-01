using PlotThoseLines.Services;

namespace PlotThoseLines.Tests.Services
{
    public class LocalAssetValidationTests
    {
        [Fact]
        public void LocalAsset_WithValidId_HasPropertiesSet()
        {
            // Arrange
            const string EXPECTED_ID = "bitcoin";
            const string EXPECTED_SYMBOL = "BTC";
            const string EXPECTED_NAME = "Bitcoin";
            
            // Act
            var asset = new LocalAsset
            {
                Id = EXPECTED_ID,
                Symbol = EXPECTED_SYMBOL,
                Name = EXPECTED_NAME
            };

            // Assert
            Assert.Equal(EXPECTED_ID, asset.Id);
            Assert.Equal(EXPECTED_SYMBOL, asset.Symbol);
            Assert.Equal(EXPECTED_NAME, asset.Name);
        }

        [Fact]
        public void LocalAsset_WithHistoryData_PriceIsAccessible()
        {
            // Arrange
            const string ASSET_ID = "bitcoin";
            const string EXPECTED_DATE = "2024-01-01";
            const double EXPECTED_PRICE = 42000.0;
            const int EXPECTED_DATA_COUNT = 1;
            
            // Act
            var asset = new LocalAsset
            {
                Id = ASSET_ID,
                HistoryData = new List<LocalAssetHistoryData>
                {
                    new LocalAssetHistoryData { Date = EXPECTED_DATE, Price = EXPECTED_PRICE }
                }
            };

            // Assert
            Assert.NotNull(asset.HistoryData);
            Assert.Equal(EXPECTED_DATA_COUNT, asset.HistoryData.Count);
            Assert.Equal(EXPECTED_DATE, asset.HistoryData[0].Date);
            Assert.Equal(EXPECTED_PRICE, asset.HistoryData[0].Price);
        }
    }
}
