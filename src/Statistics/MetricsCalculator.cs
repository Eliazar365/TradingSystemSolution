using System.Linq;
using System.Threading.Tasks;
using Market;
using Traders;

namespace Statistics
{
    public class MetricsCalculator
    {
        private readonly PriceBook _priceBook;
        private readonly TransactionLog _transactionLog;

        public MetricsCalculator(PriceBook priceBook, TransactionLog transactionLog)
        {
            _priceBook = priceBook;
            _transactionLog = transactionLog;
        }

        // Calcula métricas en tiempo real
        public async Task RunAsync()
        {
            while (true)
            {
                var transactions = _transactionLog.GetTransactions();
                if (transactions.Length > 0)
                {
                    decimal averagePrice = transactions.Average(t => t.Price);
                    int volume = transactions.Length;
                    Console.WriteLine($"Métricas: Precio promedio = {averagePrice:C}, Volumen = {volume}");
                }
                await Task.Delay(1000);
            }
        }
    }
}