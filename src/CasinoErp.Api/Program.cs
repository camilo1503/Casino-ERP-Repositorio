using System.Text.Json;
using System.Text.Json.Serialization;
using CasinoErp.Api;
using CasinoErp.Infraestructura;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AgregarCasinoErp(builder.Configuration);
builder.Services
    .AddControllers(o => o.Filters.Add<FiltroExcepciones>())
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper)));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new() { Title = "Casino ERP API", Version = "v1" }));

var origenes = builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? ["http://localhost:4200"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.WithOrigins(origenes).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.Services.PrepararBaseDatos(builder.Configuration.GetValue("BaseDatos:CargarDatosDemo", true));

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
// El front del ERP (wwwroot/index.html) se sirve desde la misma API: http://localhost:5080
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();

/// <summary>Expuesto para las pruebas de integración (WebApplicationFactory).</summary>
public partial class Program;
