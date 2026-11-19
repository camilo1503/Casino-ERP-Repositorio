using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using CasinoErp.Api;
using CasinoErp.Api.Seguridad;
using CasinoErp.Infraestructura;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AgregarCasinoErp(builder.Configuration);
builder.Services
    .AddControllers(o => o.Filters.Add<FiltroExcepciones>())
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Casino ERP API", Version = "v1" });
    // HU-06: botón "Authorize" para pegar el token de POST /api/auth/login
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", In = ParameterLocation.Header,
        Description = "Token de POST /api/auth/login",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }] = [],
    });
});

// HU-06: sesión con JWT y autorización por módulo (Plan-Ivan I5)
builder.Services.AddOptions<OpcionesJwt>().Bind(builder.Configuration.GetSection("Jwt"))
    .Validate(o => o.Clave.Length >= 32, "Jwt:Clave debe tener al menos 32 caracteres").ValidateOnStart();
builder.Services.AddSingleton<JwtEmisor>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<OpcionesJwt>>((o, jwt) =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Value.Emisor,
            ValidAudience = jwt.Value.Audiencia,
            IssuerSigningKey = JwtEmisor.Llave(jwt.Value),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = JwtEmisor.ClaimNombre,
            RoleClaimType = ClaimTypes.Role,
        };
        o.Events = new JwtBearerEvents { OnTokenValidated = JwtEmisor.ValidarUsuarioVigente };
    });
builder.Services.AddAuthorization();

var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.Services.PrepararBaseDatos(builder.Configuration);

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
// El front del ERP (wwwroot/index.html) se sirve desde la misma API: http://localhost:5080
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
// Prueba de vida para el front (js/api/nucleo.js). Debe seguir siendo anónima cuando se active la seguridad (HU-06).
app.MapGet("/api/salud", () => Results.Ok(new { estado = "ok" })).AllowAnonymous().ExcludeFromDescription();
app.MapControllers();

app.Run();

/// <summary>Expuesto para las pruebas de integración (WebApplicationFactory).</summary>
public partial class Program;
