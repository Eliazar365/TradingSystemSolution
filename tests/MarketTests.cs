using Market;
using Xunit;

namespace Tests
{
    public class MarketTests
    {
        [Fact]
        public void PriceBook_UpdatesAndGetsPrice_Correctly()
        {
            // Arrange
            var priceBook = new PriceBook();

            // Act
            priceBook.UpdatePrice("AAPL", 100.0m);
            var price = priceBook.GetPrice("AAPL");

            // Assert
            Assert.Equal(100.0m, price);
        }

        [Fact]
        public void PriceBook_ReturnsZero_IfSymbolDoesNotExist()
        {
            // Arrange
            var priceBook = new PriceBook();

            // Act
            var price = priceBook.GetPrice("GOOG");

            // Assert
            Assert.Equal(0, price);
        }
    }
}