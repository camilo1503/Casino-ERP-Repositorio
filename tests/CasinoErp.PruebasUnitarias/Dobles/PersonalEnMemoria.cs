using CasinoErp.Dominio.Personal;

namespace CasinoErp.PruebasUnitarias.Dobles;

/// <summary>Repositorio de empleados en memoria para las pruebas unitarias de Personal.</summary>
public class EmpleadosEnMemoria : IEmpleadoRepositorio
{
    public readonly List<Empleado> Datos = [];
    public int Guardados { get; private set; }

    public Task<bool> ExisteDocumentoAsync(string documento) => Task.FromResult(Datos.Any(e => e.Documento == documento));
    public Task<Empleado?> ObtenerPorDocumentoAsync(string documento) => Task.FromResult(Datos.FirstOrDefault(e => e.Documento == documento));
    public Task<IReadOnlyList<Empleado>> ListarAsync(bool soloActivos = false) =>
        Task.FromResult<IReadOnlyList<Empleado>>(Datos.Where(e => !soloActivos || e.Activo).ToList());
    public Task AgregarAsync(Empleado empleado) { empleado.Id = Datos.Count + 1; Datos.Add(empleado); Guardados++; return Task.CompletedTask; }
    public Task GuardarCambiosAsync() { Guardados++; return Task.CompletedTask; }
}
