using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Simulador
{
    private const int TOTAL_TAREAS = 6;
    private readonly int[] configuracionesNucleos = { 2, 4, 6, 8, 16 };
    private readonly ConcurrentDictionary<string, decimal> precios = new()
    {
        ["USD"] = 1.00m,
        ["EUR"] = 1.10m,
        ["BTC"] = 30000.0m,
        ["ETH"] = 2000.0m
    };
    private readonly Random random = new();
    private readonly object transaccionLock = new();
    private CancellationTokenSource _ctsPrecios; // Nuevo: TokenSource para controlar la actualización

    public async Task EjecutarPruebas()
    {
        Console.WriteLine("Iniciando simulador con mercado dinámico...");

        // Nuevo: Iniciar actualización de precios con cancelación controlada
        _ctsPrecios = new CancellationTokenSource();
        var actualizadorPrecios = Task.Run(() => ActualizarPrecios(_ctsPrecios.Token), _ctsPrecios.Token);

        try
        {
            // Modo Secuencial (referencia)
            Console.WriteLine("\n[PRUEBA SECUENCIAL - 1 NÚCLEO]");
            var referenciaSecuencial = await EjecutarPruebaSegura(
                nucleos: 1,
                paralelo: false);

            // Pruebas paralelas
            var resultadosParalelos = new List<ResultadosSesion>();

            foreach (int nucleos in configuracionesNucleos)
            {
                Console.WriteLine($"\n[PRUEBA PARALELA - {nucleos} NÚCLEOS]");
                var resultado = await EjecutarPruebaSegura(
                    nucleos: nucleos,
                    paralelo: true);

                resultadosParalelos.Add(resultado);
            }

            GenerarReporteEscalabilidad(referenciaSecuencial, resultadosParalelos);
        }
        finally
        {
            // Nuevo: Detener limpiamente la actualización de precios
            _ctsPrecios.Cancel();
            try
            {
                await actualizadorPrecios;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\nActualización de precios detenida correctamente");
            }
            _ctsPrecios.Dispose();
        }
    }

    // Actualización de precios 
    private async Task ActualizarPrecios(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(2000, ct); // Tiempo para que se muestre la actualizacion de monedas

                lock (transaccionLock)
                {
                    foreach (var moneda in precios.Keys.ToList())
                    {
                        decimal cambio = (decimal)(random.NextDouble() - 0.5) * 0.05m;
                        precios[moneda] = Math.Round(precios[moneda] * (1 + cambio), 2);
                    }

                    Console.WriteLine("\nPRECIOS ACTUALIZADOS:");
                    foreach (var kvp in precios)
                    {
                        Console.WriteLine($"   {kvp.Key}: {kvp.Value} USD");
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Salir cuando se cancele
                break;
            }
        }
    }

    private async Task<ResultadosSesion> EjecutarPruebaSegura(int nucleos, bool paralelo)
    {
        var cts = new CancellationTokenSource();
        try
        {
            var tareaEjecucion = Task.Run(async () =>
            {
                var sesion = new SesionTrading(
                    totalTareas: TOTAL_TAREAS,
                    nucleosSolicitados: nucleos,
                    precios: precios,
                    paralelo: paralelo,
                    random: random,
                    transaccionLock: transaccionLock);

                return await sesion.Ejecutar(cts.Token);
            }, cts.Token);

            var tareaTimeout = Task.Delay(TimeSpan.FromMinutes(5), cts.Token);

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