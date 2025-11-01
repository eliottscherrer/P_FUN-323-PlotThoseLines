using PlotThoseLines.Services;

namespace PlotThoseLines.Tests.Services
{
    public class SettingsServiceTests
    {
        [Fact]
        public void GetApiKey_ReturnsNonNullValue()
        {
            // Arrange
            var service = new SettingsService();

            // Act
            var apiKey = service.GetApiKey();

            // Assert
            Assert.NotNull(apiKey);
            // Note: GetApiKey() can return empty string if no key is set, which is valid behavior, that's why we only check for NotNull here.
        }

        [Fact]
        public async Task SetApiKeyAsync_SavesAndRetrievesKey()
        {
            // Arrange
            var service = new SettingsService();
            const string TEST_API_KEY = "test-api-key-123";

            // Act
            await service.SetApiKeyAsync(TEST_API_KEY);
            var retrievedKey = service.GetApiKey();

            // Assert
            Assert.Equal(TEST_API_KEY, retrievedKey);
        }
    }
}
