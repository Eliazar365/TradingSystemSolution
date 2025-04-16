using System;
using System.Threading;

namespace Market
{
    public static class MarketSimulator
    {
        // Ejecuta la simulación del mercado
        public static void Run(PriceBook priceBook)
        {
            Random rand = new();
            string symbol = "AAPL";
            decimal initialPrice = 100.0m;

            while (true)
            {
                // Simula fluctuaciones de precio
                decimal variation = (decimal)(rand.NextDouble() * 2 - 1);
                initialPrice += variation;
                priceBook.UpdatePrice(symbol, initialPrice);
                Thread.Sleep(50);
            }
        }
    }
}