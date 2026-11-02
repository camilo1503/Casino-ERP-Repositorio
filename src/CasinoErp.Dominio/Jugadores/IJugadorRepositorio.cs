namespace CasinoErp.Dominio.Jugadores;

public interface IJugadorRepositorio
{
    Task<bool> ExisteDocumentoAsync(string documento);
    Task<bool> ExisteCorreoAsync(string correo);
    Task<Jugador?> ObtenerPorDocumentoAsync(string documento);
    Task<IReadOnlyList<Jugador>> ListarAsync(string? busqueda = null);
    Task AgregarAsync(Jugador jugador);
    Task GuardarCambiosAsync();
}
