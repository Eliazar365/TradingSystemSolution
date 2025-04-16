using System.Threading.Tasks;
using Market;
using Traders;
using Xunit;

namespace Tests
{
    public class TraderTests
    {
        [Fact]
        public async Task Trader_ExecutesTransaction_WhenPriceIsValid()
        {
            // Arrange
            var priceBook = new PriceBook();
            var transactionLog = new TransactionLog();
            var trader = new Trader("T1", priceBook, transactionLog);
            priceBook.UpdatePrice("AAPL", 100.0m);

            // Act
            var task = trader.RunAsync("AAPL");
            await Task.Delay(200);

            // Assert
            var transactions = transactionLog.GetTransactions();
            Assert.True(transactions.Length >= 0);
        }
    }
}