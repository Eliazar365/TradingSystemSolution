using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Simulador
{
    private const int TOTAL_TAREAS = 200;
    private readonly int[] configuracionesNucleos = {20 };

    public async Task EjecutarPruebas()
    {
        var monedas = new[] { "USD", "EUR", "BTC" };
        var preciosIniciales = new ConcurrentDictionary<string, decimal>
        {
            ["USD"] = 1.0m,
            ["EUR"] = 0.93m,
            ["BTC"] = 42500.0m
        };

        // Modo Secuencial 
        Console.WriteLine("\n[PRUEBA SECUENCIAL - 1 NÚCLEO]");
        var referenciaSecuencial = await EjecutarPruebaSegura(
            nucleos: 1,
            precios: preciosIniciales,
            paralelo: false);

        // Pruebas paralelas
        var resultadosParalelos = new List<ResultadosSesion>();

        foreach (int nucleos in configuracionesNucleos)
        {
            Console.WriteLine($"\n[PRUEBA PARALELA - {nucleos} NÚCLEOS]");
            var resultado = await EjecutarPruebaSegura(
                nucleos: nucleos,
                precios: preciosIniciales,
                paralelo: true);

            resultadosParalelos.Add(resultado);
        }

        GenerarReporteEscalabilidad(referenciaSecuencial, resultadosParalelos);
    }

    private async Task<ResultadosSesion> EjecutarPruebaSegura(int nucleos,
        ConcurrentDictionary<string, decimal> precios, bool paralelo)
    {
        using var cts = new CancellationTokenSource();
        try
        {
            var tareaEjecucion = Task.Run(async () =>
            {
                var sesion = new SesionTrading(
                    totalTareas: TOTAL_TAREAS,
                    nucleosSolicitados: nucleos,
                    precios: precios,
                    paralelo: paralelo);

                return await sesion.Ejecutar(cts.Token);
            }, cts.Token);

            //Tiempo para hacer que el programa se detenga en caso de que se acabe el tiempo
            var tareaTimeout = Task.Delay(TimeSpan.FromMinutes(1), cts.Token);

            await Task.WhenAny(tareaEjecucion, tareaTimeout);

            if (tareaTimeout.IsCompleted)
            {
                cts.Cancel();
                throw new TimeoutException("Prueba excedió el tiempo límite");
            }

            return await tareaEjecucion;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Prueba cancelada correctamente");
            throw;
        }
    }

    private void GenerarReporteEscalabilidad(ResultadosSesion referencia, List<ResultadosSesion> pruebas)
    {
        Console.WriteLine("\nREPORTE DE ESCALABILIDAD");
        Console.WriteLine("====================================================================");
        Console.WriteLine($"| {"CONFIGURACIÓN",-12} | {"DURACIÓN",8} | {"TAREAS/SEG",9} | {"ACELERACIÓN",11} | {"EFICIENCIA",10} |");
        Console.WriteLine("====================================================================");

        Console.WriteLine($"| {"Secuencial",-12} | {referencia.Duracion.TotalSeconds,6:N2}s | {referencia.OperacionesPorSegundo,8:N2} | {"-",11} | {"-",10} |");

        foreach (var prueba in pruebas)
        {
            var (aceleracion, eficiencia) = ResultadosSesion.CalcularMetricas(referencia, prueba);

            Console.WriteLine($"| {prueba.NucleosUsados + " núcleos",-12} | {prueba.Duracion.TotalSeconds,6:N2}s | " +
                            $"{prueba.OperacionesPorSegundo,8:N2} | {aceleracion,9:N2}x | {eficiencia,8:N2}% |");
        }

        Console.WriteLine("====================================================================");
    }
}