using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

class SesionTrading
{
    private readonly int totalTareas;
    private readonly int nucleosSolicitados;
    private readonly ConcurrentDictionary<string, decimal> precios;
    private readonly bool modoParalelo;

    public SesionTrading(int totalTareas, int nucleosSolicitados,
        ConcurrentDictionary<string, decimal> precios, bool paralelo)
    {
        this.totalTareas = totalTareas;
        this.nucleosSolicitados = nucleosSolicitados;
        this.precios = precios;
        modoParalelo = paralelo;
    }

    public async Task<ResultadosSesion> Ejecutar(CancellationToken ct)
    {
        var cronometro = Stopwatch.StartNew();
        int tareasCompletadas = 0;

        if (modoParalelo)
        {
            await EjecutarTareasParalelas(ct, () => Interlocked.Increment(ref tareasCompletadas));
        }
        else
        {
            await EjecutarTareasSecuenciales(ct, () => tareasCompletadas++);
        }

        cronometro.Stop();
        return new ResultadosSesion(
            cronometro.Elapsed,
            tareasCompletadas,
            modoParalelo ? "PARALELO" : "SECUENCIAL",
            nucleosSolicitados);
    }

    private async Task EjecutarTareasParalelas(CancellationToken ct, Action incrementarContador)
    {
        var opciones = new ParallelOptions
        {
            MaxDegreeOfParallelism = nucleosSolicitados,
            CancellationToken = ct
        };

        await Task.Run(() =>
        {
            Parallel.For(0, totalTareas, opciones, i =>
            {
                var trader = new Trader(i % nucleosSolicitados + 1, precios);
                trader.EjecutarOperacion();
                incrementarContador();
            });
        }, ct);
    }

    private async Task EjecutarTareasSecuenciales(CancellationToken ct, Action incrementarContador)
    {
        for (int i = 0; i < totalTareas && !ct.IsCancellationRequested; i++)
        {
            var trader = new Trader(1, precios);
            await trader.EjecutarOperacionAsync(ct);
            incrementarContador();
        }
    }
}