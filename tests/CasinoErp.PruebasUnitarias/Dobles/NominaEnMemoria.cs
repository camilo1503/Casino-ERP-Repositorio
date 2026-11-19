using CasinoErp.Dominio.Nominas;

namespace CasinoErp.PruebasUnitarias.Dobles;

/// <summary>Parámetros de nómina en memoria (un registro por año).</summary>
public class ParametrosEnMemoria : IParametrosNominaRepositorio
{
    public readonly List<ParametrosNomina> Datos = [];

    public Task<ParametrosNomina?> ObtenerAsync(int anio) => Task.FromResult(Datos.FirstOrDefault(p => p.Anio == anio));

    public Task GuardarAsync(ParametrosNomina p)
    {
        Datos.RemoveAll(x => x.Anio == p.Anio);
        if (p.Id == 0) p.Id = Datos.Count + 1;
        Datos.Add(p);
        return Task.CompletedTask;
    }
}

/// <summary>Nóminas en memoria para las pruebas del servicio.</summary>
public class NominasEnMemoria : INominaRepositorio
{
    public readonly List<Nomina> Datos = [];
    private int _siguienteId = 1;
    public int Guardados { get; private set; }

    public Task<Nomina?> ObtenerAsync(int id) => Task.FromResult(Datos.FirstOrDefault(n => n.Id == id));
    public Task<Nomina?> ObtenerPorPeriodoAsync(int anio, int mes) => Task.FromResult(Datos.FirstOrDefault(n => n.Anio == anio && n.Mes == mes));
    public Task<IReadOnlyList<Nomina>> ListarAsync() =>
        Task.FromResult<IReadOnlyList<Nomina>>(Datos.OrderByDescending(n => n.Anio).ThenByDescending(n => n.Mes).ToList());
    public Task AgregarAsync(Nomina nomina) { nomina.Id = _siguienteId++; Datos.Add(nomina); Guardados++; return Task.CompletedTask; }
    public Task EliminarAsync(Nomina nomina) { Datos.Remove(nomina); Guardados++; return Task.CompletedTask; }
    public Task GuardarCambiosAsync() { Guardados++; return Task.CompletedTask; }
}
