using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

class Trader
{
    private readonly int id;
    private readonly ConcurrentDictionary<string, decimal> precios;
    private readonly Random aleatorio = new();
    private readonly object bloqueoConsola = new();

    public Trader(int id, ConcurrentDictionary<string, decimal> precios)
    {
        this.id = id;
        this.precios = precios;
    }

    public void EjecutarOperacion()
    {
        var operacion = GenerarOperacion();
        //Tiempo entre cada transaccion
        Thread.Sleep(2000);

        lock (bloqueoConsola)
        {
            Console.WriteLine($"Trader #{id} | {operacion.Moneda} | " +
                             $"{operacion.Tipo} | {operacion.Monto:N2} | " +
                             $"{operacion.Precio:N2}");
        }
    }

    public async Task EjecutarOperacionAsync(CancellationToken ct)
    {
        var operacion = GenerarOperacion();
        //Tiempo entre cada transaccion
        await Task.Delay(2000, ct);

        Console.WriteLine($"Trader #1 | {operacion.Moneda} | " +
                        $"{operacion.Tipo} | {operacion.Monto:N2} | " +
                        $"{operacion.Precio:N2}");
    }

    private (string Moneda, string Tipo, double Monto, decimal Precio) GenerarOperacion()
    {
        string[] monedas = precios.Keys.ToArray();
        string moneda = monedas[aleatorio.Next(monedas.Length)];
        string tipo = aleatorio.Next(2) == 0 ? "COMPRA" : "VENTA";
        double monto = Math.Round(aleatorio.NextDouble() * 10 + 1, 2);
        decimal precio = precios[moneda];

        return (moneda, tipo, monto, precio);
    }
}