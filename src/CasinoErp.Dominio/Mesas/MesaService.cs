using System.Text.RegularExpressions;
using CasinoErp.Dominio.Comun;

namespace CasinoErp.Dominio.Mesas;

/// <summary>E-02 Catálogo de mesas: registrar, consultar, actualizar, abrir/cerrar y retirar (borrado lógico).</summary>
public class MesaService
{
    public const string MensajeRegistrada = "Mesa registrada exitosamente";
    public const string MensajeActualizada = "Mesa actualizada";
    public const string MensajeAbierta = "Mesa abierta";
    public const string MensajeCerrada = "Mesa cerrada";
    public const string MensajeRetirada = "Mesa retirada del catálogo";
    public const string MensajeMantenimiento = "Mesa enviada a mantenimiento";
    public const string MensajeReparada = "Mesa lista para abrir";

    public static readonly IReadOnlyList<string> Juegos = ["Blackjack", "Ruleta", "Póker", "Baccarat", "Craps", "Tragamonedas"];
    public static readonly IReadOnlyList<string> Zonas = ["Zona A", "Zona B", "Salón VIP"];

    /// <summary>Mesas de juego M-00; bancos de tragamonedas TR-0.</summary>
    private static readonly Regex Numero = new(@"^(M-\d{2}|TR-\d{1,2})$");

    private readonly IMesaRepositorio _repo;

    public MesaService(IMesaRepositorio repo) => _repo = repo;

    public static void ValidarLimites(long apuestaMinima, long apuestaMaxima)
    {
        if (apuestaMinima <= 0) throw new ReglaNegocioException("La apuesta mínima debe ser mayor que cero");
        if (apuestaMinima >= apuestaMaxima) throw new ReglaNegocioException("La apuesta mínima debe ser menor que la máxima");
    }

    public async Task<Mesa> RegistrarAsync(string numero, string juego, long apuestaMinima, long apuestaMaxima, string zona = "Zona A")
    {
        numero = numero?.Trim().ToUpperInvariant() ?? "";
        if (!Numero.IsMatch(numero)) throw new ReglaNegocioException("El número de mesa debe tener el formato M-00");
        if (!Juegos.Contains(juego)) throw new ReglaNegocioException("El juego es obligatorio");
        if (!Zonas.Contains(zona)) throw new ReglaNegocioException("La ubicación no es válida");
        ValidarLimites(apuestaMinima, apuestaMaxima);
        if (await _repo.ExisteNumeroAsync(numero)) throw new ReglaNegocioException("El número de mesa ya está registrado");

        var mesa = new Mesa
        {
            Numero = numero,
            Juego = juego,
            Zona = zona,
            Asientos = juego == "Tragamonedas" ? 20 : 7,
            ApuestaMinima = apuestaMinima,
            ApuestaMaxima = apuestaMaxima,
            Estado = EstadoMesa.Cerrada,
        };
        await _repo.AgregarAsync(mesa);
        return mesa;
    }

    public async Task<Mesa> ConsultarAsync(string numero) =>
        await _repo.ObtenerPorNumeroAsync(numero) ?? throw new NoEncontradoException("Mesa no encontrada");

    public Task<IReadOnlyList<Mesa>> ListarAsync() => _repo.ListarAsync();

    public async Task<Mesa> ActualizarLimitesAsync(string numero, long apuestaMinima, long apuestaMaxima, string? zona = null)
    {
        var mesa = await ConsultarActivaAsync(numero);
        ValidarLimites(apuestaMinima, apuestaMaxima);
        if (zona is not null && !Zonas.Contains(zona)) throw new ReglaNegocioException("La ubicación no es válida");
        mesa.ApuestaMinima = apuestaMinima;
        mesa.ApuestaMaxima = apuestaMaxima;
        if (zona is not null) mesa.Zona = zona;
        await _repo.GuardarCambiosAsync();
        return mesa;
    }

    public async Task<Mesa> AbrirAsync(string numero)
    {
        var mesa = await ConsultarActivaAsync(numero);
        if (mesa.Estado == EstadoMesa.Abierta) throw new ReglaNegocioException("La mesa ya está abierta");
        if (mesa.Estado == EstadoMesa.Mantenimiento) throw new ReglaNegocioException("La mesa está en mantenimiento");
        mesa.Estado = EstadoMesa.Abierta;
        await _repo.GuardarCambiosAsync();
        return mesa;
    }

    public async Task<Mesa> CerrarAsync(string numero)
    {
        var mesa = await ConsultarActivaAsync(numero);
        if (mesa.Estado != EstadoMesa.Abierta) throw new ReglaNegocioException("La mesa ya está cerrada");
        mesa.Estado = EstadoMesa.Cerrada;
        await _repo.GuardarCambiosAsync();
        return mesa;
    }

    public async Task<Mesa> EnviarAMantenimientoAsync(string numero)
    {
        var mesa = await ConsultarActivaAsync(numero);
        if (mesa.Estado == EstadoMesa.Abierta) throw new ReglaNegocioException("Cierra la mesa antes de enviarla a mantenimiento");
        mesa.Estado = EstadoMesa.Mantenimiento;
        await _repo.GuardarCambiosAsync();
        return mesa;
    }

    public async Task<Mesa> MarcarReparadaAsync(string numero)
    {
        var mesa = await ConsultarActivaAsync(numero);
        if (mesa.Estado != EstadoMesa.Mantenimiento) throw new ReglaNegocioException("La mesa no está en mantenimiento");
        mesa.Estado = EstadoMesa.Cerrada;
        await _repo.GuardarCambiosAsync();
        return mesa;
    }

    public async Task<Mesa> RetirarAsync(string numero, string? motivo)
    {
        var mesa = await ConsultarActivaAsync(numero);
        if (mesa.Estado == EstadoMesa.Abierta) throw new ReglaNegocioException("No se puede retirar una mesa abierta");
        if (string.IsNullOrWhiteSpace(motivo)) throw new ReglaNegocioException("El motivo es obligatorio");
        mesa.Estado = EstadoMesa.Retirada;
        mesa.MotivoRetiro = motivo.Trim();
        await _repo.GuardarCambiosAsync();
        return mesa;
    }

    private async Task<Mesa> ConsultarActivaAsync(string numero)
    {
        var mesa = await ConsultarAsync(numero);
        if (mesa.Estado == EstadoMesa.Retirada) throw new ReglaNegocioException("La mesa está retirada");
        return mesa;
    }
}
