using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

class Trader
{
    private readonly int id;
    private readonly ConcurrentDictionary<string, decimal> precios;
    private readonly Random random;
    private readonly object transaccionLock;

    public Trader(int id, ConcurrentDictionary<string, decimal> precios,
        Random random, object transaccionLock)
    {
        this.id = id;
        this.precios = precios;
        this.random = random;
        this.transaccionLock = transaccionLock;
    }
    private (string Moneda, string Tipo, decimal Monto, decimal Precio) GenerarOperacion()
    {
        string[] monedas = precios.Keys.ToArray();
        string moneda = monedas[random.Next(monedas.Length)];
        string tipo = random.Next(2) == 0 ? "COMPRA" : "VENTA";
        decimal monto = Math.Round((decimal)random.NextDouble() * 10m + 1m, 2);
        decimal precio = precios[moneda];

        return (moneda, tipo, monto, precio);
    }
    public void EjecutarOperacion(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return;

        var operacion = GenerarOperacion();
        Thread.Sleep(200); // tiempo entre cada transaccion 

        lock (transaccionLock)
        {
            // Impacto en el mercado
            decimal cambio = (decimal)(random.NextDouble() - 0.4) * 0.01m;
            precios[operacion.Moneda] = Math.Round(
                precios[operacion.Moneda] * (1 + (operacion.Tipo == "COMPRA" ? cambio : -cambio)),
                2);

            Console.WriteLine($"Trader #{id} {operacion.Tipo} {operacion.Monto} " +
                            $"{operacion.Moneda} {operacion.Precio:N2} " +
                            $"(Nuevo precio: {precios[operacion.Moneda]:N2})");
        }
    }
    public async Task EjecutarOperacionAsync(CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
            return;

        var operacion = GenerarOperacion();

        await Task.Delay(200, ct); //tiempo entre cada transaccion 

        lock (transaccionLock)
        {
            // Impacto en el mercado
            decimal cambio = (decimal)(random.NextDouble() - 0.4) * 0.01m;
            precios[operacion.Moneda] = Math.Round(
                precios[operacion.Moneda] * (1 + (operacion.Tipo == "COMPRA" ? cambio : -cambio)),
                2);

            Console.WriteLine($"Trader #1 {operacion.Tipo} {operacion.Monto} " +
                            $"{operacion.Moneda} {operacion.Precio:N2} " +
                            $"(Nuevo precio: {precios[operacion.Moneda]:N2})");
        }
    }
}