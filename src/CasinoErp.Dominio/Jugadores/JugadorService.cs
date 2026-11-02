using System.Text.RegularExpressions;
using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Mesas;

namespace CasinoErp.Dominio.Jugadores;

/// <summary>E-01 Gestión de perfiles de jugador: CRUD con borrado lógico.</summary>
public class JugadorService
{
    public const string MensajeRegistrado = "Jugador registrado exitosamente";
    public const string MensajeActualizado = "Perfil actualizado";
    public const string MensajeDesactivado = "Jugador desactivado";
    public const string MensajeAutoexcluido = "Jugador autoexcluido";
    public const string MensajeReactivado = "Jugador reactivado";
    public const int EdadMinima = 18;

    public static readonly IReadOnlyList<string> TiposDocumento = ["CC", "CE", "Pasaporte"];
    public static readonly IReadOnlyList<string> Franjas = ["Mañana", "Tarde", "Noche"];

    private static readonly Regex Correo = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    private static readonly Regex DocumentoNumerico = new(@"^\d{6,12}$");
    private static readonly Regex Pasaporte = new(@"^[A-Z0-9]{6,12}$");

    private readonly IJugadorRepositorio _repo;
    private readonly TimeProvider _reloj;

    public JugadorService(IJugadorRepositorio repo, TimeProvider reloj)
    {
        _repo = repo;
        _reloj = reloj;
    }

    public static void ValidarCorreo(string? correo)
    {
        if (!Correo.IsMatch(correo?.Trim() ?? "")) throw new ReglaNegocioException("Formato de correo inválido");
    }

    /// <summary>Edad cumplida en la fecha de referencia.</summary>
    public static int CalcularEdad(DateOnly nacimiento, DateOnly hoy)
    {
        var edad = hoy.Year - nacimiento.Year;
        if (hoy < nacimiento.AddYears(edad)) edad--;
        return edad;
    }

    public static bool DocumentoValido(string tipoDocumento, string? documento) =>
        tipoDocumento == "Pasaporte" ? Pasaporte.IsMatch(documento ?? "") : DocumentoNumerico.IsMatch(documento ?? "");

    public async Task<Jugador> RegistrarAsync(string documento, string nombre, string correo, DateOnly fechaNacimiento, string tipoDocumento = "CC")
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre es obligatorio");
        if (!TiposDocumento.Contains(tipoDocumento)) throw new ReglaNegocioException("El tipo de documento no es válido");
        documento = documento?.Trim().ToUpperInvariant() ?? "";
        if (!DocumentoValido(tipoDocumento, documento)) throw new ReglaNegocioException("El documento no es válido");
        ValidarCorreo(correo);
        if (await _repo.ExisteDocumentoAsync(documento)) throw new ReglaNegocioException("El documento ya está registrado");
        if (CalcularEdad(fechaNacimiento, Hoy()) < EdadMinima)
            throw new ReglaNegocioException($"El jugador debe ser mayor de {EdadMinima} años");
        if (await _repo.ExisteCorreoAsync(correo.Trim().ToLowerInvariant())) throw new ReglaNegocioException("Ese correo ya está registrado");

        var jugador = new Jugador
        {
            TipoDocumento = tipoDocumento,
            Documento = documento,
            Nombre = nombre.Trim(),
            Correo = correo.Trim().ToLowerInvariant(),
            FechaNacimiento = fechaNacimiento,
            Estado = EstadoJugador.Activo,
            FechaRegistro = _reloj.GetUtcNow().UtcDateTime,
        };
        await _repo.AgregarAsync(jugador);
        return jugador;
    }

    public async Task<Jugador> ConsultarAsync(string documento) =>
        await _repo.ObtenerPorDocumentoAsync(documento) ?? throw new NoEncontradoException("Jugador no encontrado");

    public Task<IReadOnlyList<Jugador>> ListarAsync(string? busqueda = null) => _repo.ListarAsync(busqueda);

    public async Task<Jugador> ActualizarAsync(string documento, string nombre, string correo)
    {
        var jugador = await ConsultarAsync(documento);
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre es obligatorio");
        ValidarCorreo(correo);
        jugador.Nombre = nombre.Trim();
        jugador.Correo = correo.Trim().ToLowerInvariant();
        await _repo.GuardarCambiosAsync();
        return jugador;
    }

    /// <summary>HU-JUG-03: datos de contacto y preferencias de juego. El documento no se edita.</summary>
    public async Task<Jugador> ActualizarPerfilAsync(string documento, string? telefono, string? ciudad, string franja,
        long apuestaPromedio, IReadOnlyCollection<string> preferencias)
    {
        var jugador = await ConsultarAsync(documento);
        if (preferencias.Count == 0) throw new ReglaNegocioException("Elige al menos un juego preferido");
        if (preferencias.Any(p => !MesaService.Juegos.Contains(p))) throw new ReglaNegocioException("Juego preferido no válido");
        if (string.IsNullOrWhiteSpace(telefono)) throw new ReglaNegocioException("El teléfono es obligatorio");
        if (!Franjas.Contains(franja)) throw new ReglaNegocioException("El horario preferido no es válido");
        if (apuestaPromedio < 1000) throw new ReglaNegocioException("La apuesta promedio debe ser de al menos $1.000");

        jugador.Telefono = telefono.Trim();
        jugador.Ciudad = string.IsNullOrWhiteSpace(ciudad) ? jugador.Ciudad : ciudad.Trim();
        jugador.Franja = franja;
        jugador.ApuestaPromedio = apuestaPromedio;
        jugador.Preferencias = preferencias.Distinct().ToList();
        await _repo.GuardarCambiosAsync();
        return jugador;
    }

    public Task<Jugador> DesactivarAsync(string documento, string? motivo) =>
        CambiarEstadoAsync(documento, motivo, EstadoJugador.Inactivo);

    public Task<Jugador> AutoexcluirAsync(string documento, string? motivo) =>
        CambiarEstadoAsync(documento, motivo, EstadoJugador.Autoexcluido);

    public async Task<Jugador> ReactivarAsync(string documento)
    {
        var jugador = await ConsultarAsync(documento);
        if (jugador.Estado == EstadoJugador.Autoexcluido)
            throw new ReglaNegocioException("Un jugador autoexcluido no se puede reactivar desde el ERP");
        jugador.Estado = EstadoJugador.Activo;
        jugador.MotivoEstado = null;
        await _repo.GuardarCambiosAsync();
        return jugador;
    }

    /// <summary>Suma puntos de lealtad (1 punto por cada $10.000 comprados en caja).</summary>
    public async Task SumarPuntosAsync(Jugador jugador, long montoCompra)
    {
        jugador.Puntos += (int)Math.Round(montoCompra / 10_000m);
        await _repo.GuardarCambiosAsync();
    }

    private async Task<Jugador> CambiarEstadoAsync(string documento, string? motivo, EstadoJugador estado)
    {
        if (string.IsNullOrWhiteSpace(motivo)) throw new ReglaNegocioException("El motivo es obligatorio");
        var jugador = await ConsultarAsync(documento);
        jugador.Estado = estado;
        jugador.MotivoEstado = motivo.Trim();
        await _repo.GuardarCambiosAsync();
        return jugador;
    }

    private DateOnly Hoy() => DateOnly.FromDateTime(_reloj.GetLocalNow().DateTime);
}
