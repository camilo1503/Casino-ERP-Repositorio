using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CasinoErp.PruebasIntegracion;

/// <summary>Login de pruebas con las cuentas de demostración (contrato C3, llega con la I5 de Iván).</summary>
public static class Autenticacion
{
    public const string ContrasenaDemo = "Casino2026";
    public const string CodigoDemo = "482913";

    /// <summary>Devuelve un cliente con <c>Authorization: Bearer</c> del usuario indicado.</summary>
    public static Task<HttpClient> ClienteComoAsync(this CasinoErpFactory factory, string correo) =>
        ((WebApplicationFactory<Program>)factory).ClienteComoAsync(correo);

    /// <summary>Igual que la anterior, para factories derivadas con <c>WithWebHostBuilder</c>.</summary>
    public static Task<HttpClient> ClienteComoAsync(this WebApplicationFactory<Program> factory, string correo) =>
        ClienteComoAsync(factory, correo, ContrasenaDemo);

    /// <summary>Login con una contraseña distinta de la de demostración (por ejemplo, una temporal).</summary>
    public static async Task<HttpClient> ClienteComoAsync(this WebApplicationFactory<Program> factory, string correo, string contrasena)
    {
        var cliente = factory.CreateClient();
        var respuesta = await cliente.PostAsJsonAsync("/api/auth/login", new { correo, contrasena, codigo = CodigoDemo });
        if (!respuesta.IsSuccessStatusCode)
            throw new InvalidOperationException($"No se pudo iniciar sesión como {correo}: {(int)respuesta.StatusCode} {await respuesta.Content.ReadAsStringAsync()}");
        var token = (await respuesta.LeerJsonAsync()).GetProperty("token").GetString();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }
}
