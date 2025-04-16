using System.Collections.Concurrent;
using System.Threading.Tasks;
using Traders;

namespace Market
{
    public class TransactionLog
    {
        private readonly ConcurrentBag<Transaction> _transactions = new();

        // A�ade una transacci�n de forma as�ncrona
        public async Task AddTransactionAsync(Transaction transaction)
        {
            _transactions.Add(transaction);
            await Task.CompletedTask;
        }

        // Obtiene todas las transacciones
        public Transaction[] GetTransactions()
        {
            return _transactions.ToArray();
        }
    }
}