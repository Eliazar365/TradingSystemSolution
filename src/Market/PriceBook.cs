using System.Collections.Concurrent;

namespace Market
{
    public class PriceBook
    {
        private readonly ConcurrentDictionary<string, decimal> _prices = new();

        // Actualiza el precio de un activo
        public void UpdatePrice(string symbol, decimal price)
        {
            _prices[symbol] = price;
        }

        // Obtiene el precio de un activo
        public decimal GetPrice(string symbol)
        {
            return _prices.TryGetValue(symbol, out var price) ? price : 0;
        }
    }
}