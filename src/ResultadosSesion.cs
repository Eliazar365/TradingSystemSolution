using System;

record ResultadosSesion(
    TimeSpan Duracion,
    int TareasCompletadas,
    string Modo,
    int NucleosUsados
)
{
    public double OperacionesPorSegundo => TareasCompletadas / Duracion.TotalSeconds;

    public static (double aceleracion, double eficiencia) CalcularMetricas(
        ResultadosSesion referencia,
        ResultadosSesion prueba)
    {
        if (referencia.TareasCompletadas == 0 || prueba.TareasCompletadas == 0)
            return (0, 0);

        double aceleracion = referencia.Duracion.TotalSeconds / prueba.Duracion.TotalSeconds;
        double eficiencia = (aceleracion / prueba.NucleosUsados) * 100;

        return (aceleracion, eficiencia);
    }
}