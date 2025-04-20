using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

class Trader
{
    private readonly int id;
    private readonly ConcurrentDictionary<string, decimal> precios;
    private readonly Random aleatorio = new();

    public Trader(int id, ConcurrentDictionary<string, decimal> precios)
    {
        this.id = id;
        this.precios = precios;
    }

    public void EjecutarOperacion()
    {
        var operacion = GenerarOperacion();
        Thread.Sleep(200); // Simula trabajo

        lock (Console.Out)
        {
            Console.WriteLine($"Tarea #{id} | {operacion.Moneda} | " +
                             $"{operacion.Tipo} | {operacion.Monto:N2} | " +
                             $"{operacion.Precio:N2}");
        }
    }

    public async Task EjecutarOperacionAsync()
    {
        var operacion = GenerarOperacion();
        await Task.Delay(200); // Simula trabajo asíncrono

        Console.WriteLine($"Tarea #1 | {operacion.Moneda} | " +
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