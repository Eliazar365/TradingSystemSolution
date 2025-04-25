using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("SIMULADOR DE TRADING CON MERCADO DINÁMICO");
        Console.WriteLine("--------------------------------------------\n");

        try
        {
            var simulador = new Simulador();
            await simulador.EjecutarPruebas();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }

        Console.WriteLine("\nSimulación completada. Presione cualquier tecla para salir...");
        Console.ReadKey();
    }
}