using System.Net.Http.Json;
using System.Text.Json;

namespace CasinoErp.PruebasIntegracion;

/// <summary>Lectura del cuerpo JSON de las respuestas de la API (contrato C3).</summary>
public static class ApiJson
{
    public static async Task<JsonElement> LeerJsonAsync(this HttpResponseMessage respuesta) =>
        await respuesta.Content.ReadFromJsonAsync<JsonElement>();
}
