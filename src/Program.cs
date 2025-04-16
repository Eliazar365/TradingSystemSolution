using System;
using System.Linq;
using System.Threading.Tasks;
using Market;
using Statistics;
using Traders;

namespace MarketSimulation
{
    class Program
    {
        static async Task Main()
        {
            try
            {
                var priceBook = new PriceBook();
                var transactionLog = new TransactionLog();
                var metricsCalculator = new MetricsCalculator(priceBook, transactionLog);

                // Iniciar simulación del mercado
                var marketTask = Task.Run(() => MarketSimulator.Run(priceBook));

                // Iniciar traders
                var traders = new Trader[]
                {
                    new Trader("T1", priceBook, transactionLog),
                    new Trader("T2", priceBook, transactionLog),
                    new Trader("T3", priceBook, transactionLog)
                };
                var traderTasks = traders.Select(t => t.RunAsync("AAPL")).ToArray();

                // Iniciar cálculo de métricas
                var metricsTask = metricsCalculator.RunAsync();

                // Ejecutar todas las tareas en paralelo
                await Task.WhenAll(traderTasks.Concat(new[] { marketTask, metricsTask }).ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en la simulación: {ex.Message}");
            }
        }
    }
}