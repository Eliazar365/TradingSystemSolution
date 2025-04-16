using System;
using System.Threading.Tasks;
using Market;

namespace Traders
{
    public class Trader
    {
        private readonly string _id;
        private readonly PriceBook _priceBook;
        private readonly TransactionLog _transactionLog;

        public Trader(string id, PriceBook priceBook, TransactionLog transactionLog)
        {
            _id = id;
            _priceBook = priceBook;
            _transactionLog = transactionLog;
        }

        // Ejecuta el trader de forma asíncrona
        public async Task RunAsync(string symbol)
        {
            Random rand = new();
            while (true)
            {
                decimal price = _priceBook.GetPrice(symbol);
                if (price > 0 && rand.NextDouble() < 0.3)
                {
                    var transaction = new Transaction(_id, symbol, price);
                    await _transactionLog.AddTransactionAsync(transaction);
                }
                await Task.Delay(100);
            }
        }
    }
}