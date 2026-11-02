using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Mesas;

namespace CasinoErp.Dominio.Rellenos;

/// <summary>Operación de mesas: solicitud y aprobación de rellenos contra el saldo de la bóveda (Taller 3).</summary>
public class RellenoService
{
    public const string MensajeSolicitado = "Relleno solicitado";
    public const string MensajeAprobado = "Relleno aprobado";
    public const string MensajeRechazado = "Relleno rechazado";
    public const string MensajeDeposito = "Depósito registrado en la bóveda";

    private readonly IRellenoRepositorio _repo;
    private readonly MesaService _mesas;
    private readonly TimeProvider _reloj;

    public RellenoService(IRellenoRepositorio repo, MesaService mesas, TimeProvider reloj)
    {
        _repo = repo;
        _mesas = mesas;
        _reloj = reloj;
    }

    public async Task<Relleno> SolicitarAsync(string mesaNumero, long monto, string? solicitadoPor = null)
    {
        if (monto <= 0) throw new ReglaNegocioException("El monto debe ser mayor que cero");
        var mesa = await _mesas.ConsultarAsync(mesaNumero);
        if (mesa.Estado != EstadoMesa.Abierta) throw new ReglaNegocioException("Solo las mesas abiertas piden relleno");
        if (await _repo.ExistePendienteAsync(mesa.Numero)) throw new ReglaNegocioException("La mesa ya tiene un relleno pendiente");

        var relleno = new Relleno
        {
            MesaNumero = mesa.Numero,
            Monto = monto,
            Estado = EstadoRelleno.Pendiente,
            SolicitadoEn = _reloj.GetUtcNow().UtcDateTime,
            SolicitadoPor = string.IsNullOrWhiteSpace(solicitadoPor) ? "Crupier" : solicitadoPor.Trim(),
        };
        await _repo.AgregarAsync(relleno);
        return relleno;
    }

    public async Task<Relleno> AprobarAsync(int id, string? aprobadoPor)
    {
        var relleno = await ConsultarPendienteAsync(id);
        var boveda = await _repo.ObtenerBovedaAsync();
        if (relleno.Monto > boveda.Saldo) throw new ReglaNegocioException("Saldo insuficiente en la bóveda");

        boveda.Saldo -= relleno.Monto;
        relleno.Estado = EstadoRelleno.Aprobado;
        relleno.ResueltoPor = string.IsNullOrWhiteSpace(aprobadoPor) ? "Jefe de piso" : aprobadoPor.Trim();
        await _repo.GuardarCambiosAsync();
        return relleno;
    }

    public async Task<Relleno> RechazarAsync(int id, string? motivo, string? rechazadoPor)
    {
        if (string.IsNullOrWhiteSpace(motivo)) throw new ReglaNegocioException("El motivo es obligatorio");
        var relleno = await ConsultarPendienteAsync(id);
        relleno.Estado = EstadoRelleno.Rechazado;
        relleno.MotivoRechazo = motivo.Trim();
        relleno.ResueltoPor = string.IsNullOrWhiteSpace(rechazadoPor) ? "Jefe de piso" : rechazadoPor.Trim();
        await _repo.GuardarCambiosAsync();
        return relleno;
    }

    public Task<IReadOnlyList<Relleno>> ListarAsync() => _repo.ListarAsync();

    public Task<Boveda> ConsultarBovedaAsync() => _repo.ObtenerBovedaAsync();

    public async Task<Boveda> DepositarAsync(long monto)
    {
        if (monto <= 0) throw new ReglaNegocioException("El monto debe ser mayor que cero");
        var boveda = await _repo.ObtenerBovedaAsync();
        boveda.Saldo += monto;
        await _repo.GuardarCambiosAsync();
        return boveda;
    }

    private async Task<Relleno> ConsultarPendienteAsync(int id)
    {
        var relleno = await _repo.ObtenerAsync(id) ?? throw new NoEncontradoException("Relleno no encontrado");
        if (relleno.Estado == EstadoRelleno.Aprobado) throw new ReglaNegocioException("El relleno ya está aprobado");
        if (relleno.Estado != EstadoRelleno.Pendiente) throw new ReglaNegocioException("Solo se resuelven rellenos pendientes");
        return relleno;
    }
}
