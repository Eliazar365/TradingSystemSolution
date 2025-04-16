namespace Traders
{
    public class Transaction
    {
        public string TraderId { get; }
        public string Symbol { get; }
        public decimal Price { get; }
        public DateTime Timestamp { get; }

        public Transaction(string traderId, string symbol, decimal price)
        {
            TraderId = traderId;
            Symbol = symbol;
            Price = price;
            Timestamp = DateTime.Now;
        }
    }
}