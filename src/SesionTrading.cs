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
    private readonly Random random;
    private readonly object transaccionLock;

    public SesionTrading(int totalTareas, int nucleosSolicitados,
        ConcurrentDictionary<string, decimal> precios, bool paralelo,
        Random random, object transaccionLock)
    {
        this.totalTareas = totalTareas;
        this.nucleosSolicitados = nucleosSolicitados;
        this.precios = precios;
        modoParalelo = paralelo;
        this.random = random;
        this.transaccionLock = transaccionLock;
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
                var trader = new Trader(
                    id: i % nucleosSolicitados + 1,
                    precios: precios,
                    random: random,
                    transaccionLock: transaccionLock);

                trader.EjecutarOperacion(ct);
                incrementarContador();
            });
        }, ct);
    }

    private async Task EjecutarTareasSecuenciales(CancellationToken ct, Action incrementarContador)
    {
        for (int i = 0; i < totalTareas && !ct.IsCancellationRequested; i++)
        {
            var trader = new Trader(
                id: 1,
                precios: precios,
                random: random,
                transaccionLock: transaccionLock);

            await trader.EjecutarOperacionAsync(ct);
            incrementarContador();
        }
    }
}