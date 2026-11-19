using CasinoErp.Dominio.Comun;

namespace CasinoErp.Dominio.Personal;

/// <summary>HU-04 Talento Humano: registrar, actualizar, retirar (borrado lógico) y consultar empleados.</summary>
public class EmpleadoService(IEmpleadoRepositorio repo, TimeProvider reloj)
{
    public const string MensajeRegistrado = "Empleado registrado";
    public const string MensajeActualizado = "Empleado actualizado";
    public const string MensajeRetirado = "Empleado retirado";

    /// <summary>Salarios del mockup (const SAL).</summary>
    public static readonly IReadOnlyDictionary<Cargo, long> SalarioPorCargo = new Dictionary<Cargo, long>
    {
        [Cargo.Crupier] = 2_600_000,
        [Cargo.Cajero] = 2_300_000,
        [Cargo.JefeDePiso] = 4_200_000,
    };

    public static readonly IReadOnlyList<string> JuegosCrupier = ["Blackjack", "Ruleta", "Póker", "Baccarat", "Craps"];

    /// <summary>Quita puntos, espacios y cualquier otro carácter que no sea dígito: "1.010.200.300" -> "1010200300".</summary>
    public static string NormalizarDocumento(string? documento) => new((documento ?? "").Where(char.IsDigit).ToArray());

    public async Task<Empleado> RegistrarAsync(string documento, string nombre, Cargo cargo, string? juego,
        TipoContrato tipoContrato = TipoContrato.Indefinido, long? salarioBase = null)
    {
        documento = NormalizarDocumento(documento);
        nombre = nombre?.Trim() ?? "";
        if (documento.Length == 0 || nombre.Length == 0) throw new ReglaNegocioException("Nombre y documento son obligatorios.");
        if (documento.Length is < 6 or > 10) throw new ReglaNegocioException("El documento no es válido.");
        if (cargo == Cargo.Crupier && (juego is null || !JuegosCrupier.Contains(juego)))
            throw new ReglaNegocioException("El juego del crupier no es válido.");
        if (salarioBase is <= 0) throw new ReglaNegocioException("El salario base debe ser mayor que cero.");
        if (await repo.ExisteDocumentoAsync(documento)) throw new ReglaNegocioException("Ya existe un empleado con ese documento.");

        var empleado = new Empleado
        {
            Documento = documento,
            Nombre = nombre,
            Cargo = cargo,
            Juego = cargo switch
            {
                Cargo.Crupier => juego!,
                Cargo.Cajero => "Caja",
                _ => "Supervisión",
            },
            TipoContrato = tipoContrato,
            SalarioBase = salarioBase ?? SalarioPorCargo[cargo],
            FechaIngreso = DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime),
            Turno = Turno.Tarde,
            Area = "Sin asignar",
            Activo = true,
        };
        await repo.AgregarAsync(empleado);
        return empleado;
    }

    public async Task<Empleado> ActualizarAsync(string documento, Turno turno, string area, TipoContrato tipoContrato, long salarioBase)
    {
        var empleado = await ConsultarAsync(documento);
        if (string.IsNullOrWhiteSpace(area)) throw new ReglaNegocioException("El área es obligatoria.");
        if (salarioBase <= 0) throw new ReglaNegocioException("El salario base debe ser mayor que cero.");
        if (!empleado.Activo) throw new ReglaNegocioException("El empleado está retirado.");

        empleado.Turno = turno;
        empleado.Area = area.Trim();
        empleado.TipoContrato = tipoContrato;
        empleado.SalarioBase = salarioBase;
        await repo.GuardarCambiosAsync();
        return empleado;
    }

    public async Task<Empleado> RetirarAsync(string documento, string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo)) throw new ReglaNegocioException("El motivo es obligatorio.");
        var empleado = await ConsultarAsync(documento);
        if (!empleado.Activo) throw new ReglaNegocioException("El empleado ya está retirado.");

        empleado.Activo = false;
        empleado.MotivoRetiro = motivo.Trim();
        await repo.GuardarCambiosAsync();
        return empleado;
    }

    public async Task<Empleado> ConsultarAsync(string documento) =>
        await repo.ObtenerPorDocumentoAsync(NormalizarDocumento(documento))
        ?? throw new NoEncontradoException("Empleado no encontrado");

    public Task<IReadOnlyList<Empleado>> ListarAsync(bool soloActivos = false) => repo.ListarAsync(soloActivos);
}
