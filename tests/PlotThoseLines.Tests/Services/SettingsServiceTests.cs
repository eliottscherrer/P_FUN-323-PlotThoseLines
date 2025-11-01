using PlotThoseLines.Services;

namespace PlotThoseLines.Tests.Services
{
    public class SettingsServiceTests
    {
        [Fact]
        public void GetApiKey_ReturnsNonEmptyValue()
        {
            // Arrange
            var service = new SettingsService();

            // Act
            var apiKey = service.GetApiKey();

            // Assert
            Assert.NotNull(apiKey);
            Assert.NotEmpty(apiKey);
        }
    }
}
