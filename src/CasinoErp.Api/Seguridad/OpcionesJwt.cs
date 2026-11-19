namespace CasinoErp.Api.Seguridad;

/// <summary>Sección <c>Jwt</c> de la configuración. La clave no va en el repo: en local sale de appsettings.Development.json y en Azure del app setting <c>Jwt__Clave</c> que genera Terraform.</summary>
public class OpcionesJwt
{
    public string Emisor { get; set; } = "CasinoErp";
    public string Audiencia { get; set; } = "CasinoErp.Front";
    public string Clave { get; set; } = "";
    public int HorasVigencia { get; set; } = 8;
}
