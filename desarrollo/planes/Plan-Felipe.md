# Plan Felipe — Talento Humano y Nómina (HU-04)

> **Para agentes:** antes de empezar, lee `AGENT.md`, `desarrollo/contratos.md` y tu bitácora `desarrollo/bitacoras/felipe.md`. Ejecuta las tareas en orden, con TDD estricto (prueba que falla → código → prueba que pasa → commit), y reporta según AGENT.md §8. Los pasos usan casillas (`- [ ]`).

**Objetivo:** CRUD de empleados del casino y motor de cálculo de la nómina mensual, con deducciones y aportes según el tipo de contrato, expuesto en `POST /api/nomina/calculate` y conectado a las pantallas **Competencias** y **Turnos y nómina** del mockup.

**HU de la guía:** *HU-04 Cálculo de Nómina (8 SP).* "Como jefe de RR. HH., quiero poder calcular la nómina mensual de los empleados activos, aplicando deducciones y parafiscales según su tipo de contrato (ej. cotizaciones a seguridad social, impuestos), para generar reportes de pago precisos."

**Criterios que revisa el profesor en tu código:** SRP y DIP en `PayrollEngine` (debes poder justificarlos en el PR y en arc42), pruebas exhaustivas del motor (cobertura > 80 %) y diagrama de secuencia UML del cálculo.

**Mockup de referencia** (`src/CasinoErp.Api/wwwroot/index.html`):
- `const SAL={Crupier:2600000,Cajero:2300000,'Jefe de piso':4200000}`
- `function payroll()`: recargo nocturno del 35 % solo en turno Noche; aportes del empleado del 8 % (salud 4 % + pensión 4 %)
- `V.rrhh` (pantalla Turnos y nómina), `MODALS.payroll` y `ACT.approvePayroll` (texto: "Nómina aprobada. Se pagará el 30/10/2026.")
- `MODALS.employee` y `FORM.employee` (HU-PER-01 "Registrar empleado": mensajes `Nombre y documento son obligatorios.` y `Ya existe un empleado con ese documento.`)
- `seed().staff`: los 18 empleados de demo

**Arquitectura de tu módulo:**

```
NominaController ──▶ NominaService ──▶ PayrollEngine ──▶ IReglaLiquidacion ◀── ReglaContratoLaboral
      │                    │            (cálculo puro)                      ◀── ReglaPrestacionServicios
      │                    ├──▶ IEmpleadoRepositorio
      │                    ├──▶ IParametrosNominaRepositorio
      │                    └──▶ INominaRepositorio
EmpleadosController ──▶ EmpleadoService ──▶ IEmpleadoRepositorio
```

- **SRP:** `PayrollEngine` solo calcula (sin base de datos ni reloj); `NominaService` orquesta y persiste; cada regla liquida un solo tipo de contrato.
- **DIP:** el motor depende de la abstracción `IReglaLiquidacion`, inyectada como `IEnumerable<IReglaLiquidacion>`, y el servicio depende de interfaces de repositorio. Agregar un contrato nuevo (por ejemplo, aprendiz SENA) es una clase más, sin tocar el motor (OCP).

**Archivos que son tuyos:**
- `src/CasinoErp.Dominio/Personal/**`, `src/CasinoErp.Dominio/Nominas/**`
- En `src/CasinoErp.Infraestructura/`: `Configuraciones/EmpleadoConfiguracion.cs`, `Configuraciones/NominaConfiguracion.cs`, `Repositorios/PersonalRepositorios.cs`, `Repositorios/NominaRepositorios.cs`, `Modulos/ModuloPersonal.cs`, `Modulos/ModuloNomina.cs`, `Semillas/SemillaPersonal.cs`, `Semillas/SemillaNomina.cs`
- `src/CasinoErp.Api/Controllers/EmpleadosController.cs`, `NominaController.cs`
- `src/CasinoErp.Api/wwwroot/js/api/personal.js`
- En `tests/CasinoErp.PruebasUnitarias/`: `Dobles/PersonalEnMemoria.cs`, `Dobles/NominaEnMemoria.cs`, `EmpleadoServiceTests.cs`, `ReglasLiquidacionTests.cs`, `PayrollEngineTests.cs`, `NominaServiceTests.cs`
- `tests/CasinoErp.PruebasIntegracion/PersonalNominaApiTests.cs`
- `docs/arc42/05-bloques/nomina.md`, `docs/arc42/09-decisiones/ADR-005-parametros-nomina.md`

**Contratos:** eres dueño de **C4** (Personal); Iván lo necesita, así que súbelo primero (F1). Consumes **C1**, **C2** y **C3** (Fase 0 de Jorge) y **C5** (seguridad de Iván, desde F7).

---

## Orden y dependencias

| Tarea | Necesita en `develop` | Puedes empezar |
|---|---|---|
| F0 Arranque | — | Día 1 |
| F1 Dominio de empleados (contrato C4) | — | Día 1 (archivos nuevos del dominio) |
| F2 Parámetros y reglas de liquidación | — | En paralelo con F1 o justo después |
| F3 Motor y servicio de nómina | F1, F2 | Sin esperar a la Fase 0 |
| F4 Persistencia y API de empleados | **Fase 0 (J0)** | Cuando Jorge anuncie D-007 |
| F5 Persistencia y API de nómina | F3, F4 | |
| F6 Front (`personal.js`) | Fase 0 (C2) | |
| F7 Autorización de tus controladores | **I5 de Iván** | Cuando Iván anuncie "seguridad activa" |
| F8 arc42 y cierre | F5 | |
| **F9 Acceso de Jorge a Azure (asignar roles)** | — (solo tu suscripción) | **Hoy, antes de F4**: son 5 minutos y desbloquea J2, J3, J4 y J6 de Jorge |

Mientras esperas la Fase 0, haz F1, F2 y F3, que solo crean archivos nuevos en `Dominio/` y en las pruebas unitarias.

---

### Tarea F0 · Arranque

- [ ] **F0.1** Sincronizar y crear la rama: `git switch develop; git pull --rebase origin develop; git switch -c feature/HU-04-empleados-dominio`.
- [ ] **F0.2** Ejecutar `dotnet test CasinoErp.sln` y anotar en la bitácora cuántas pruebas pasan.
- [ ] **F0.3** En Jira, mover HU-04 y la subtarea F1 a "En curso".
- [ ] **F0.4** En la bitácora, copiar la tabla de tareas F0–F8 en "Estado de mi plan".

---

### Tarea F1 · Dominio de empleados (contrato C4, PR temprano)

**Archivos:**
- Crear: `src/CasinoErp.Dominio/Personal/Empleado.cs` (los enums `Cargo`, `Turno` y `TipoContrato`, la clase `Empleado` y la interfaz `IEmpleadoRepositorio`, **exactamente** como en `desarrollo/contratos.md` §C4)
- Crear: `src/CasinoErp.Dominio/Personal/EmpleadoService.cs`
- Pruebas: `tests/CasinoErp.PruebasUnitarias/Dobles/PersonalEnMemoria.cs` (`EmpleadosEnMemoria : IEmpleadoRepositorio`, con el patrón de `JugadoresEnMemoria`: `List<Empleado> Datos`, un `Id` incremental y el contador `Guardados`) y `EmpleadoServiceTests.cs`

**Interfaces que produce:**

```csharp
public class EmpleadoService(IEmpleadoRepositorio repo, TimeProvider reloj)
{
    public const string MensajeRegistrado = "Empleado registrado";
    public const string MensajeActualizado = "Empleado actualizado";
    public const string MensajeRetirado = "Empleado retirado";
    public static readonly IReadOnlyDictionary<Cargo, long> SalarioPorCargo;   // Crupier 2_600_000, Cajero 2_300_000, JefeDePiso 4_200_000
    public static readonly IReadOnlyList<string> JuegosCrupier;                // "Blackjack","Ruleta","Póker","Baccarat","Craps"

    public Task<Empleado> RegistrarAsync(string documento, string nombre, Cargo cargo, string? juego,
                                         TipoContrato tipoContrato = TipoContrato.Indefinido, long? salarioBase = null);
    public Task<Empleado> ActualizarAsync(string documento, Turno turno, string area, TipoContrato tipoContrato, long salarioBase);
    public Task<Empleado> RetirarAsync(string documento, string? motivo);
    public Task<Empleado> ConsultarAsync(string documento);
    public Task<IReadOnlyList<Empleado>> ListarAsync(bool soloActivos = false);
}
```

**Reglas de `RegistrarAsync`, en este orden:**

| # | Regla | Mensaje (`ReglaNegocioException`) |
|---|---|---|
| 1 | Se quitan los no dígitos del documento (`"1.010.200.300"` → `"1010200300"`); nombre con `Trim()` | — |
| 2 | Documento o nombre vacíos | `Nombre y documento son obligatorios.` |
| 3 | Documento con menos de 6 o más de 10 dígitos | `El documento no es válido.` |
| 4 | Crupier con un juego que no está en `JuegosCrupier` | `El juego del crupier no es válido.` |
| 5 | `salarioBase` llega y es ≤ 0 | `El salario base debe ser mayor que cero.` |
| 6 | Ya existe ese documento | `Ya existe un empleado con ese documento.` |
| 7 | Juego final: crupier → el recibido; cajero → `"Caja"`; jefe de piso → `"Supervisión"` | — |
| 8 | Salario final: el recibido o `SalarioPorCargo[cargo]` | — |
| 9 | `FechaIngreso = DateOnly.FromDateTime(reloj.GetUtcNow().UtcDateTime)`; `Turno = Tarde`; `Area = "Sin asignar"`; `Activo = true` | — |

**Otras reglas:**
- `ActualizarAsync`: área vacía → `El área es obligatoria.`; salario ≤ 0 → `El salario base debe ser mayor que cero.`; empleado retirado → `El empleado está retirado.`
- `RetirarAsync`: motivo vacío → `El motivo es obligatorio.`; ya retirado → `El empleado ya está retirado.`; si no, `Activo = false`, se guarda `MotivoRetiro` y se llama a `GuardarCambiosAsync()`.
- `ConsultarAsync`: si no existe → `NoEncontradoException("Empleado no encontrado")`. Normaliza el documento igual que el registro.

- [ ] **F1.1 Escribir las pruebas que fallan**

```csharp
public class EmpleadoServiceTests
{
    private readonly EmpleadosEnMemoria _repo = new();
    private readonly EmpleadoService _servicio;
    public EmpleadoServiceTests() => _servicio = new EmpleadoService(_repo, new RelojFijo());

    [Fact] public async Task Registra_un_crupier_con_el_salario_de_su_cargo()
    {
        var e = await _servicio.RegistrarAsync("1.010.200.300", "Ana Torres", Cargo.Crupier, "Blackjack");
        e.Documento.Should().Be("1010200300");
        e.SalarioBase.Should().Be(2_600_000);
        e.Juego.Should().Be("Blackjack");
        e.Turno.Should().Be(Turno.Tarde);
        e.Area.Should().Be("Sin asignar");
        e.TipoContrato.Should().Be(TipoContrato.Indefinido);
        e.FechaIngreso.Should().Be(new DateOnly(2026, 10, 26));
        _repo.Datos.Should().ContainSingle();
    }
    [Fact] public async Task El_cajero_queda_con_juego_Caja_y_su_salario()        // Juego "Caja", 2_300_000
    [Fact] public async Task El_jefe_de_piso_queda_en_Supervision()               // Juego "Supervisión", 4_200_000
    [Fact] public async Task Respeta_el_salario_y_contrato_recibidos()            // PrestacionServicios, 3_000_000
    [Theory, InlineData("", "Ana Torres"), InlineData("1010200300", "  ")]
    public async Task Exige_nombre_y_documento(string doc, string nombre)         // "Nombre y documento son obligatorios."
    [Theory, InlineData("12345"), InlineData("12345678901")]
    public async Task Rechaza_documento_de_longitud_invalida(string doc)          // "El documento no es válido."
    [Fact] public async Task Rechaza_documento_duplicado_aunque_venga_con_puntos() // "1010200300" y luego "1.010.200.300" -> "Ya existe un empleado con ese documento."
    [Fact] public async Task Rechaza_crupier_con_juego_invalido()                 // "Dominó" -> "El juego del crupier no es válido."
    [Fact] public async Task Rechaza_crupier_sin_juego()                          // null -> mismo mensaje
    [Fact] public async Task Rechaza_salario_no_positivo()                        // 0 -> "El salario base debe ser mayor que cero."
    [Fact] public async Task Actualiza_turno_area_contrato_y_salario()            // Noche, "M-01", TerminoFijo, 2_700_000; Guardados se incrementa
    [Fact] public async Task Actualizar_exige_area()                              // "El área es obligatoria."
    [Fact] public async Task No_actualiza_un_retirado()                           // "El empleado está retirado."
    [Fact] public async Task Retira_con_motivo_y_conserva_el_registro()           // Activo false, MotivoRetiro guardado, sigue en ListarAsync()
    [Fact] public async Task Retirar_exige_motivo()                               // "El motivo es obligatorio."
    [Fact] public async Task No_retira_dos_veces()                                // "El empleado ya está retirado."
    [Fact] public async Task Consultar_inexistente_lanza_no_encontrado()          // NoEncontradoException "Empleado no encontrado"
    [Fact] public async Task Listar_solo_activos_excluye_retirados()
}
```

- [ ] **F1.2** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter EmpleadoServiceTests`. Esperado: error de compilación (`Empleado` no existe).
- [ ] **F1.3** Implementar `Empleado.cs` (contrato C4 literal) y `EmpleadoService.cs`.
- [ ] **F1.4** Ejecutar el mismo comando. Esperado: todas aprobadas. Luego `dotnet test CasinoErp.sln`, todo en verde.
- [ ] **F1.5** Commit, push y PR: `git add src/CasinoErp.Dominio/Personal tests/CasinoErp.PruebasUnitarias; git commit -m "HU-04: Dominio de empleados (contrato C4)"; git push -u origin HEAD`. En el PR, escribir **"Contiene el contrato C4 que necesita Iván"**. Lo fusiona el integrador (D-009). En la bitácora: entrada "F1 Terminada" con el enlace del PR.

---

### Tarea F2 · Parámetros de nómina y reglas de liquidación

**Rama:** `feature/HU-04-reglas-liquidacion`, tomada de `develop` (después del merge de F1) o de la rama de F1 si todavía no se ha fusionado.

**Archivos:**
- Crear en `src/CasinoErp.Dominio/Nominas/`: `ParametrosNomina.cs`, `Nomina.cs`, `IReglaLiquidacion.cs`, `ReglaContratoLaboral.cs`, `ReglaPrestacionServicios.cs`
- Pruebas: `tests/CasinoErp.PruebasUnitarias/ReglasLiquidacionTests.cs`

**Interfaces que produce:**

```csharp
namespace CasinoErp.Dominio.Nominas;

public class ParametrosNomina
{
    public int Id { get; set; }
    public int Anio { get; set; }
    public long SalarioMinimo { get; set; }
    public long AuxilioTransporte { get; set; }
    public decimal SaludEmpleado { get; set; }      public decimal PensionEmpleado { get; set; }
    public decimal FondoSolidaridad { get; set; }   public decimal RecargoNocturno { get; set; }
    public decimal RetencionServicios { get; set; }
    public decimal SaludEmpleador { get; set; }     public decimal PensionEmpleador { get; set; }
    public decimal Arl { get; set; }                public decimal CajaCompensacion { get; set; }
    public decimal Icbf { get; set; }               public decimal Sena { get; set; }

    /// Decretos 1469 y 1470 de 2025 (ver desarrollo/decisiones/D-003).
    public static ParametrosNomina Decreto2026() => new()
    {
        Anio = 2026, SalarioMinimo = 1_750_905, AuxilioTransporte = 249_095,
        SaludEmpleado = 0.04m, PensionEmpleado = 0.04m, FondoSolidaridad = 0.01m, RecargoNocturno = 0.35m,
        RetencionServicios = 0.10m, SaludEmpleador = 0.085m, PensionEmpleador = 0.12m, Arl = 0.00522m,
        CajaCompensacion = 0.04m, Icbf = 0.03m, Sena = 0.02m,
    };
}
public interface IParametrosNominaRepositorio { Task<ParametrosNomina?> ObtenerAsync(int anio); Task GuardarAsync(ParametrosNomina p); }

public enum EstadoNomina { Borrador, Aprobada }
public class LiquidacionEmpleado
{
    public int Id { get; set; }  public int NominaId { get; set; }
    public string EmpleadoDocumento { get; set; } = "";  public string EmpleadoNombre { get; set; } = "";
    public Cargo Cargo { get; set; }  public Turno Turno { get; set; }  public TipoContrato TipoContrato { get; set; }
    public long SalarioBase { get; set; }  public long RecargoNocturno { get; set; }  public long AuxilioTransporte { get; set; }
    public long DeduccionSalud { get; set; }  public long DeduccionPension { get; set; }  public long FondoSolidaridad { get; set; }
    public long RetencionFuente { get; set; }  public long AportesEmpleador { get; set; }  public long Neto { get; set; }
    public long TotalDeducciones => DeduccionSalud + DeduccionPension + FondoSolidaridad + RetencionFuente;
}
public class Nomina
{
    public int Id { get; set; }  public int Anio { get; set; }  public int Mes { get; set; }
    public EstadoNomina Estado { get; set; } = EstadoNomina.Borrador;
    public DateTime CalculadaEn { get; set; }  public DateTime? AprobadaEn { get; set; }
    public List<LiquidacionEmpleado> Detalle { get; set; } = [];
    public long TotalNeto => Detalle.Sum(d => d.Neto);
    public long TotalDeducciones => Detalle.Sum(d => d.TotalDeducciones);
    public long TotalAportesEmpleador => Detalle.Sum(d => d.AportesEmpleador);
    public long TotalRecargos => Detalle.Sum(d => d.RecargoNocturno);
}
public interface INominaRepositorio
{
    Task<Nomina?> ObtenerAsync(int id);  Task<Nomina?> ObtenerPorPeriodoAsync(int anio, int mes);
    Task<IReadOnlyList<Nomina>> ListarAsync();  Task AgregarAsync(Nomina nomina);
    Task EliminarAsync(Nomina nomina);  Task GuardarCambiosAsync();
}
public interface IReglaLiquidacion
{
    bool Aplica(TipoContrato tipo);
    LiquidacionEmpleado Liquidar(Empleado empleado, ParametrosNomina p);
}
```

**Algoritmo de `ReglaContratoLaboral`** (aplica a `Indefinido` y `TerminoFijo`). Cada componente se redondea por separado con `R(x) = (long)Math.Round(x, MidpointRounding.AwayFromZero)`:

```
recargo    = turno == Noche ? R(base * RecargoNocturno) : 0
ibc        = base + recargo
auxilio    = ibc <= 2 * SalarioMinimo ? AuxilioTransporte : 0
salud      = R(ibc * SaludEmpleado)
pension    = R(ibc * PensionEmpleado)
solidaridad= ibc >= 4 * SalarioMinimo ? R(ibc * FondoSolidaridad) : 0
aportes    = R(ibc*SaludEmpleador) + R(ibc*PensionEmpleador) + R(ibc*Arl) + R(ibc*CajaCompensacion) + R(ibc*Icbf) + R(ibc*Sena)
neto       = ibc + auxilio - salud - pension - solidaridad
```

**Algoritmo de `ReglaPrestacionServicios`:** `retencion = R(base * RetencionServicios)`; `neto = base - retencion`; todo lo demás en 0. La liquidación copia el documento, nombre, cargo, turno, contrato y salario base del empleado.

- [ ] **F2.1 Escribir las pruebas que fallan** (helper `static Empleado E(Cargo c, Turno t, TipoContrato k, long b, string nombre = "Ana Torres")`; `P = ParametrosNomina.Decreto2026()`)

```csharp
[Fact] public void Crupier_de_tarde_recibe_auxilio_de_transporte()
{
    var l = new ReglaContratoLaboral().Liquidar(E(Cargo.Crupier, Turno.Tarde, TipoContrato.Indefinido, 2_600_000), P);
    l.RecargoNocturno.Should().Be(0);
    l.AuxilioTransporte.Should().Be(249_095);
    l.DeduccionSalud.Should().Be(104_000);
    l.DeduccionPension.Should().Be(104_000);
    l.FondoSolidaridad.Should().Be(0);
    l.Neto.Should().Be(2_641_095);
    l.AportesEmpleador.Should().Be(780_572);
}
[Fact] public void Crupier_de_noche_suma_recargo_del_35_y_pierde_el_auxilio()
{   // coincide con el mockup: 2.600.000 + 910.000 - 280.800 = 3.229.200
    var l = new ReglaContratoLaboral().Liquidar(E(Cargo.Crupier, Turno.Noche, TipoContrato.Indefinido, 2_600_000), P);
    l.RecargoNocturno.Should().Be(910_000);
    l.AuxilioTransporte.Should().Be(0);            // IBC 3.510.000 > 2 SMMLV (3.501.810)
    l.DeduccionSalud.Should().Be(140_400);
    l.DeduccionPension.Should().Be(140_400);
    l.Neto.Should().Be(3_229_200);
    l.AportesEmpleador.Should().Be(1_053_772);
}
[Fact] public void Jefe_de_piso_sin_auxilio()                       // 4_200_000 Tarde -> Auxilio 0, Salud 168_000, Neto 3_864_000 (igual que el mockup)
[Fact] public void Cajero_de_tarde()                                // 2_300_000 Tarde -> Auxilio 249_095, Salud 92_000, Neto 2_365_095
[Fact] public void Auxilio_aplica_en_exactamente_dos_salarios_minimos()   // base 3_501_810 Tarde -> Auxilio 249_095
[Fact] public void Auxilio_no_aplica_un_peso_por_encima()           // base 3_501_811 -> Auxilio 0
[Fact] public void Fondo_de_solidaridad_en_exactamente_cuatro_salarios_minimos()  // base 7_003_620 -> FondoSolidaridad 70_036
[Fact] public void Fondo_de_solidaridad_no_aplica_un_peso_por_debajo()            // base 7_003_619 -> 0
[Fact] public void Salario_alto_paga_solidaridad()                  // 8_000_000 -> Solidaridad 80_000, Neto 7_280_000
[Fact] public void Termino_fijo_usa_la_regla_laboral()              // Aplica(TerminoFijo) true; Aplica(Indefinido) true; Aplica(PrestacionServicios) false
[Fact] public void La_regla_de_servicios_solo_aplica_a_prestacion() // Aplica(PrestacionServicios) true; las demás false
[Fact] public void Prestacion_de_servicios_solo_retiene_el_10()
{
    var l = new ReglaPrestacionServicios().Liquidar(E(Cargo.Crupier, Turno.Noche, TipoContrato.PrestacionServicios, 3_000_000), P);
    l.RecargoNocturno.Should().Be(0);
    l.AuxilioTransporte.Should().Be(0);
    l.DeduccionSalud.Should().Be(0);
    l.RetencionFuente.Should().Be(300_000);
    l.AportesEmpleador.Should().Be(0);
    l.Neto.Should().Be(2_700_000);
}
[Fact] public void La_liquidacion_copia_los_datos_del_empleado()    // documento, nombre, cargo, turno, contrato y base
[Fact] public void Redondea_hacia_arriba_en_medio_peso()            // base 1_750_910 Noche -> 1_750_910*0.35 = 612_818.5 -> Recargo 612_819 (con redondeo bancario daría 612_818)
```

- [ ] **F2.2** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter ReglasLiquidacionTests`. Esperado: error de compilación.
- [ ] **F2.3** Implementar las cinco clases e interfaces.
- [ ] **F2.4** Ejecutar el mismo comando. Esperado: todas aprobadas.
- [ ] **F2.5** Commit y push: `HU-04: Parametros de nomina 2026 y reglas de liquidacion por tipo de contrato`.

---

### Tarea F3 · `PayrollEngine` (cálculo puro) y `NominaService` (orquestación)

**Archivos:**
- Crear: `src/CasinoErp.Dominio/Nominas/PayrollEngine.cs`, `NominaService.cs`
- Pruebas: `tests/CasinoErp.PruebasUnitarias/Dobles/NominaEnMemoria.cs` (`ParametrosEnMemoria`, `NominasEnMemoria`), `PayrollEngineTests.cs`, `NominaServiceTests.cs`

**Interfaces que produce:**

```csharp
public class PayrollEngine(IEnumerable<IReglaLiquidacion> reglas)
{
    public Nomina Calcular(int anio, int mes, IEnumerable<Empleado> empleados, ParametrosNomina parametros);
}

public class NominaService(PayrollEngine motor, IEmpleadoRepositorio empleados, IParametrosNominaRepositorio parametros,
                           INominaRepositorio nominas, TimeProvider reloj)
{
    public const string MensajeCalculada = "Nómina calculada";
    public const string MensajeAprobada = "Nómina aprobada";
    public const string MensajeParametros = "Parámetros de nómina guardados";
    public Task<Nomina> CalcularAsync(int anio, int mes);
    public Task<Nomina> AprobarAsync(int id);
    public Task<Nomina> ConsultarAsync(int anio, int mes);
    public Task<IReadOnlyList<Nomina>> ListarAsync();
    public Task<ParametrosNomina> ConsultarParametrosAsync(int anio);
    public Task<ParametrosNomina> GuardarParametrosAsync(ParametrosNomina p);
}
```

**Reglas del motor (`Calcular`):**
1. Si el mes no está entre 1 y 12 → `El mes debe estar entre 1 y 12.`
2. Se toman solo los empleados con `Activo`. Si no queda ninguno → `No hay empleados activos para liquidar.`
3. Por cada empleado con contrato `Indefinido` o `TerminoFijo` y `SalarioBase < parametros.SalarioMinimo` → `El salario de {Nombre} es menor al salario mínimo de {anio}.` La prestación de servicios **no** tiene esta restricción.
4. Por cada empleado se usa `reglas.First(r => r.Aplica(e.TipoContrato))`. Si no hay ninguna regla, se lanza `InvalidOperationException($"No hay regla de liquidación para {tipo}")`, porque es un error de configuración y no de negocio.
5. Devuelve `new Nomina { Anio, Mes, Estado = Borrador, Detalle = liquidaciones ordenadas por EmpleadoNombre }`. No asigna fechas: lo hace el servicio.

**Reglas del servicio:**
- `CalcularAsync`:
  1. Si ya existe la nómina del periodo y está `Aprobada` → `La nómina de {mes:D2}/{anio} ya está aprobada.`
  2. Carga los parámetros del año; si no hay → `No hay parámetros de nómina para {anio}.`
  3. Ejecuta el motor con `empleados.ListarAsync(soloActivos: true)`.
  4. Si ya había un borrador del mismo periodo, `EliminarAsync(anterior)`.
  5. `CalculadaEn = reloj.GetUtcNow().UtcDateTime`, `AgregarAsync` y devuelve la nómina.
- `AprobarAsync`: si no existe → `NoEncontradoException("Nómina no encontrada")`; si ya está aprobada → `La nómina ya está aprobada.`; si no, `Estado = Aprobada`, `AprobadaEn = reloj` y `GuardarCambiosAsync`.
- `ConsultarAsync`: si no existe → `NoEncontradoException("Nómina no encontrada")`.
- `ConsultarParametrosAsync`: si no existe → `NoEncontradoException($"No hay parámetros de nómina para {anio}.")`.
- `GuardarParametrosAsync`: si alguna tasa está fuera de [0, 1] → `Las tasas deben estar entre 0 y 1.`; si `SalarioMinimo` ≤ 0 → `El salario mínimo debe ser mayor que cero.`; si `AuxilioTransporte` < 0 → `El auxilio de transporte no puede ser negativo.`; si `Anio` < 2020 → `El año no es válido.`

- [ ] **F3.1 Escribir las pruebas que fallan del motor**

`_motor = new PayrollEngine([new ReglaContratoLaboral(), new ReglaPrestacionServicios()])`. Empleados: Ana Torres (crupier tarde 2,6 M), Bruno Lara (crupier noche 2,6 M), Daniela Vargas (jefe de piso 4,2 M) y Pedro Ruiz (**retirado**).

```csharp
[Fact] public void Liquida_solo_empleados_activos_y_suma_totales()
{
    var n = _motor.Calcular(2026, 10, empleados, P);
    n.Detalle.Should().HaveCount(3);
    n.Detalle.Select(d => d.EmpleadoNombre).Should().BeInAscendingOrder();
    n.TotalNeto.Should().Be(2_641_095 + 3_229_200 + 3_864_000);
    n.TotalRecargos.Should().Be(910_000);
    n.Estado.Should().Be(EstadoNomina.Borrador);
    n.Anio.Should().Be(2026); n.Mes.Should().Be(10);
}
[Fact] public void Usa_la_regla_de_cada_tipo_de_contrato()             // un PrestacionServicios -> RetencionFuente 300_000; los laborales 0
[Theory, InlineData(0), InlineData(13)] public void Rechaza_mes_invalido(int mes)   // "El mes debe estar entre 1 y 12."
[Fact] public void Rechaza_cuando_no_hay_activos()                     // "No hay empleados activos para liquidar."
[Fact] public void Rechaza_salario_laboral_bajo_el_minimo()            // base 1_500_000 -> "El salario de Ana Torres es menor al salario mínimo de 2026."
[Fact] public void Prestacion_de_servicios_bajo_el_minimo_si_se_liquida()   // base 1_000_000 -> no lanza; Neto 900_000
[Fact] public void Sin_regla_para_el_contrato_es_error_de_configuracion()  // new PayrollEngine([new ReglaContratoLaboral()]) + un PrestacionServicios -> InvalidOperationException
```

- [ ] **F3.2 Escribir las pruebas que fallan del servicio**

```csharp
[Fact] public async Task Calcula_y_guarda_en_borrador()                // CalculadaEn == 2026-10-26 15:00 UTC; NominasEnMemoria tiene 1
[Fact] public async Task Recalcular_un_borrador_lo_reemplaza()         // dos CalcularAsync(2026,10) -> ListarAsync() tiene 1
[Fact] public async Task No_recalcula_un_mes_aprobado()                // "La nómina de 10/2026 ya está aprobada."
[Fact] public async Task Aprueba_la_nomina()                           // Estado Aprobada, AprobadaEn == reloj
[Fact] public async Task No_aprueba_dos_veces()                        // "La nómina ya está aprobada."
[Fact] public async Task Aprobar_inexistente_lanza_no_encontrado()     // NoEncontradoException "Nómina no encontrada"
[Fact] public async Task Falla_si_no_hay_parametros_del_anio()         // CalcularAsync(2027,1) -> "No hay parámetros de nómina para 2027."
[Fact] public async Task Meses_distintos_conviven()                    // 2026/9 y 2026/10 -> ListarAsync() tiene 2
[Fact] public async Task Guardar_parametros_valida_tasas()             // SaludEmpleado 1.5m -> "Las tasas deben estar entre 0 y 1."
[Fact] public async Task Guardar_parametros_valida_salario_minimo()    // 0 -> "El salario mínimo debe ser mayor que cero."
[Fact] public async Task Guardar_y_consultar_parametros()              // Decreto2026() con AuxilioTransporte 250_000 -> ConsultarParametrosAsync(2026).AuxilioTransporte == 250_000
```

- [ ] **F3.3** Ejecutar `dotnet test tests/CasinoErp.PruebasUnitarias --filter "PayrollEngineTests|NominaServiceTests"`. Esperado: error de compilación.
- [ ] **F3.4** Implementar `PayrollEngine` y `NominaService`.
- [ ] **F3.5** Ejecutar el mismo comando; luego la suite completa y el comando de cobertura (AGENT.md §10). Esperado: todo verde y cobertura ≥ 80 %. Anotar el porcentaje en la bitácora.
- [ ] **F3.6** Commit, push y PR (F2 + F3): `HU-04: PayrollEngine (calculo puro, SRP/DIP) y NominaService`. En la descripción del PR, explicar en tres viñetas dónde se ven SRP, DIP y OCP: es la pregunta de la retroalimentación de la guía.

---

### Tarea F4 · Persistencia y API de empleados (requiere la Fase 0)

**Rama:** `feature/HU-04-empleados-api`, tomada de `develop` después del merge de la Fase 0 y de F1.

**Archivos:**
- Crear: `src/CasinoErp.Infraestructura/Configuraciones/EmpleadoConfiguracion.cs`, `Repositorios/PersonalRepositorios.cs`, `Semillas/SemillaPersonal.cs`
- Modificar: `src/CasinoErp.Infraestructura/Modulos/ModuloPersonal.cs`
- Crear: `src/CasinoErp.Api/Controllers/EmpleadosController.cs`
- Pruebas: `tests/CasinoErp.PruebasIntegracion/PersonalNominaApiTests.cs`

**Decisiones de implementación:**
- `EmpleadoConfiguracion : IEntityTypeConfiguration<Empleado>`: tabla `Empleados`; `HasIndex(Documento).IsUnique()`; `Documento` máx. 10 y requerido; `Nombre` máx. 120 y requerido; `Juego` y `Area` máx. 30; `Cargo`, `Turno` y `TipoContrato` como `string` (`HasConversion<string>()`, máx. 25); `MotivoRetiro` máx. 250.
- `EmpleadoRepositorio(CasinoErpDbContext db) : IEmpleadoRepositorio` usa `db.Set<Empleado>()`. `ListarAsync` ordena por `Id`. `AgregarAsync` hace `Add` + `SaveChangesAsync`.
- `ModuloPersonal.AgregarPersonal()`: `AddScoped<IEmpleadoRepositorio, EmpleadoRepositorio>()`, `AddScoped<EmpleadoService>()` y `AddScoped<ISemilla, SemillaPersonal>()`.
- `SemillaPersonal : ISemilla` con `Orden = 10`. Si `db.Set<Empleado>().Any()`, no hace nada. Si no, carga los 18 empleados de `seed().staff` del mockup **en el mismo orden**, con `Documento = (1010200300 + i * 7919).ToString()`, el nombre y cargo del mockup (`'Jefe de piso'` → `Cargo.JefeDePiso`), el juego del mockup (`'Caja'` y `'Supervisión'` tal cual), `Turno = Tarde` salvo Bruno Lara (`Noche`), el área del mockup (`M-01`, `Caja 2`, `Piso A`, `Sin asignar`…), `SalarioBase = SalarioPorCargo[cargo]`, `Indefinido` y `FechaIngreso = new DateOnly(2025, 1, 15)`. Los documentos deben coincidir con la tabla de `contratos.md` §C4.
- `EmpleadosController(EmpleadoService servicio)`, ruta `api/empleados`:

| Verbo y ruta | Cuerpo | Respuesta |
|---|---|---|
| `GET /api/empleados?soloActivos=false` | — | `IReadOnlyList<Empleado>` |
| `GET /api/empleados/{documento}` | — | `Empleado` |
| `POST /api/empleados` | `RegistrarEmpleadoRequest(string Documento, string Nombre, Cargo Cargo, string? Juego, TipoContrato TipoContrato = Indefinido, long? SalarioBase = null)` | 201 `CreatedAtAction` con `Respuesta<Empleado>("Empleado registrado", e)` |
| `PUT /api/empleados/{documento}` | `ActualizarEmpleadoRequest(Turno Turno, string Area, TipoContrato TipoContrato, long SalarioBase)` | `Respuesta<Empleado>("Empleado actualizado", e)` |
| `POST /api/empleados/{documento}/retirar` | `MotivoEmpleadoRequest(string? Motivo)` | `Respuesta<Empleado>("Empleado retirado", e)` |

- [ ] **F4.1 Escribir las pruebas de integración que fallan** (si I5 ya está en `develop`, el cliente se crea con `await factory.ClienteComoAsync("felipe.cortes@pitboss.co")`; si no, con `factory.CreateClient()`)

```csharp
[Fact] public async Task Lista_los_18_empleados_de_demostracion()
{
    var lista = await _cliente.GetFromJsonAsync<JsonElement>("/api/empleados");
    lista.GetArrayLength().Should().BeGreaterOrEqualTo(18);
    lista.EnumerateArray().Should().Contain(e => e.GetProperty("documento").GetString() == "1010334923"
                                                && e.GetProperty("turno").GetString() == "NOCHE");   // Bruno Lara
}
[Fact] public async Task Registra_y_consulta_un_empleado()        // POST {documento:"1099888777", nombre:"Marta Gil", cargo:"CAJERO"} -> 201; GET /api/empleados/1099888777 -> juego "Caja"
[Fact] public async Task Documento_duplicado_responde_400()       // { mensaje: "Ya existe un empleado con ese documento." }
[Fact] public async Task Empleado_inexistente_responde_404()      // GET /api/empleados/999999 -> 404 { mensaje: "Empleado no encontrado" }
[Fact] public async Task Retira_un_empleado()                     // POST .../retirar {motivo:"Renuncia"} -> activo false
```

- [ ] **F4.2** Ejecutar `dotnet test tests/CasinoErp.PruebasIntegracion --filter PersonalNominaApiTests`. Esperado: falla (404 en las rutas).
- [ ] **F4.3** Implementar la configuración, el repositorio, el módulo, la semilla y el controlador.
- [ ] **F4.4** Ejecutar `dotnet test CasinoErp.sln`. Esperado: todo verde.
- [ ] **F4.5** Probar a mano: `$env:BaseDatos__RecrearAlIniciar="true"; dotnet run --project src/CasinoErp.Api` y en Swagger ejecutar `GET /api/empleados`. Esperado: 18 empleados.
- [ ] **F4.6** Commit, push y PR: `HU-04: Persistencia, semilla y API REST de empleados`.

---

### Tarea F5 · Persistencia y API de nómina

**Rama:** `feature/HU-04-nomina-api`.

**Archivos:**
- Crear: `src/CasinoErp.Infraestructura/Configuraciones/NominaConfiguracion.cs` (tres clases: `NominaConfiguracion`, `LiquidacionEmpleadoConfiguracion` y `ParametrosNominaConfiguracion`), `Repositorios/NominaRepositorios.cs`, `Semillas/SemillaNomina.cs`
- Modificar: `src/CasinoErp.Infraestructura/Modulos/ModuloNomina.cs`
- Crear: `src/CasinoErp.Api/Controllers/NominaController.cs`
- Pruebas: `PersonalNominaApiTests.cs` (se amplía)

**Decisiones de implementación:**
- `Nomina`: tabla `Nominas`, `HasIndex(n => new { n.Anio, n.Mes }).IsUnique()`, `Estado` como `string`; `HasMany(n => n.Detalle).WithOne().HasForeignKey(d => d.NominaId).OnDelete(DeleteBehavior.Cascade)`; `Ignore` en las propiedades `Total*`.
- `LiquidacionEmpleado`: tabla `LiquidacionesNomina`; los enums como `string`; `Ignore(TotalDeducciones)`.
- `ParametrosNomina`: tabla `ParametrosNomina`; `HasIndex(Anio).IsUnique()`; todas las tasas con `HasPrecision(7, 5)`.
- `NominaRepositorio`: `ObtenerAsync` y `ObtenerPorPeriodoAsync` con `Include(n => n.Detalle)`; `ListarAsync` con `Include` y orden descendente por `Anio`, `Mes`; `EliminarAsync` con `Remove` + `SaveChangesAsync`.
- `ParametrosNominaRepositorio.GuardarAsync`: si ya existe el año, copia los valores (*upsert*); si no, `Add`. Luego `SaveChangesAsync`.
- `ModuloNomina.AgregarNomina()`: `AddSingleton<IReglaLiquidacion, ReglaContratoLaboral>()`, `AddSingleton<IReglaLiquidacion, ReglaPrestacionServicios>()`, `AddSingleton<PayrollEngine>()`, los repositorios scoped, `AddScoped<NominaService>()` y `AddScoped<ISemilla, SemillaNomina>()`.
- `SemillaNomina : ISemilla` con `Orden = 20`: guarda `ParametrosNomina.Decreto2026()` si la tabla está vacía.
- **Serialización:** las propiedades calculadas (`TotalNeto`, `TotalDeducciones`, `TotalAportesEmpleador`, `TotalRecargos`) sí deben aparecer en el JSON, porque `System.Text.Json` serializa propiedades de solo lectura. Hay que verificarlo en la prueba.
- `NominaController(NominaService servicio)`, ruta `api/nomina`:

| Verbo y ruta | Cuerpo | Respuesta |
|---|---|---|
| `POST /api/nomina/calculate` | `CalcularNominaRequest(int Anio, int Mes)` | 200 `Respuesta<Nomina>("Nómina calculada", n)` |
| `GET /api/nomina` | — | `IReadOnlyList<Nomina>` |
| `GET /api/nomina/{anio:int}/{mes:int}` | — | `Nomina` |
| `POST /api/nomina/{id:int}/aprobar` | — | `Respuesta<Nomina>("Nómina aprobada", n)` |
| `GET /api/nomina/parametros/{anio:int}` | — | `ParametrosNomina` |
| `PUT /api/nomina/parametros/{anio:int}` | `ParametrosNomina` (se fuerza `Anio = anio`) | `Respuesta<ParametrosNomina>("Parámetros de nómina guardados", p)` |

- [ ] **F5.1 Escribir las pruebas de integración que fallan**

```csharp
[Fact] public async Task Calcula_la_nomina_de_octubre_con_los_empleados_de_demostracion()
{
    var r = await _cliente.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 10 });
    r.StatusCode.Should().Be(HttpStatusCode.OK);
    var j = await r.LeerJsonAsync();
    j.GetProperty("mensaje").GetString().Should().Be("Nómina calculada");
    var datos = j.GetProperty("datos");
    datos.GetProperty("estado").GetString().Should().Be("BORRADOR");
    datos.GetProperty("detalle").GetArrayLength().Should().Be(18);
    datos.GetProperty("totalNeto").GetInt64().Should().BeGreaterThan(0);
    datos.GetProperty("detalle").EnumerateArray()
         .Single(d => d.GetProperty("empleadoDocumento").GetString() == "1010334923")
         .GetProperty("neto").GetInt64().Should().Be(3_229_200);       // Bruno Lara, turno noche
}
[Fact] public async Task Aprueba_y_luego_no_permite_recalcular()   // aprobar -> 200; calculate otra vez -> 400 "La nómina de 10/2026 ya está aprobada."
[Fact] public async Task Mes_invalido_responde_400()               // mes 13 -> { mensaje: "El mes debe estar entre 1 y 12." }
[Fact] public async Task Consulta_los_parametros_2026()            // salarioMinimo 1750905, auxilioTransporte 249095
```

En las pruebas que aprueban, usar un mes distinto (por ejemplo, 2026/11) para no chocar con las demás pruebas de la misma clase.

- [ ] **F5.2** Ejecutar `dotnet test tests/CasinoErp.PruebasIntegracion --filter PersonalNominaApiTests`. Esperado: fallan las 4 nuevas.
- [ ] **F5.3** Implementar.
- [ ] **F5.4** Ejecutar `dotnet test CasinoErp.sln` y el comando de cobertura. Esperado: verde y ≥ 80 %.
- [ ] **F5.5** Commit, push y PR: `HU-04: Persistencia de nomina y POST /api/nomina/calculate`.

---

### Tarea F6 · Front: empleados y nómina (`js/api/personal.js`)

**Rama:** `feature/HU-04-front`. **Solo se edita `src/CasinoErp.Api/wwwroot/js/api/personal.js`** (contrato C2; no se toca `index.html`).

**Comportamiento que se implementa:**

1. **Carga `empleados` (orden 15).**
   - `obtener`: si `!S.user` o `['competencias','rrhh','mesas'].some(allowed)`, llama a `PitBossApi.pedir('GET', '/empleados?soloActivos=true')`; si no, devuelve `null`.
   - `aplicar(lista)`: por cada empleado de la API se busca en `S.staff` el que tenga el mismo documento (comparando `docApi(s.doc)` con `e.documento`).
     - Si existe, se **conserva su `id`** (`s1`…`s18`, que usan `S.users[].sid` y `table.dealer`) y sus campos de certificación (`last`, `score`, `exp`, `revoked`).
     - Si no existe, se crea con `id = 'se' + e.id`, `last: null`, `score: null`, `exp: null`, `revoked: null`.
     - En ambos casos se asignan: `name = e.nombre`; `role` = `{CRUPIER:'Crupier', CAJERO:'Cajero', JEFE_DE_PISO:'Jefe de piso'}[e.cargo]`; `game = e.juego`; `shift` = `{MANANA:'Mañana', TARDE:'Tarde', NOCHE:'Noche'}[e.turno]`; `area = e.area`; `doc = grp(+e.documento)`; `apiDoc = e.documento`; `contrato = e.tipoContrato`; `salario = e.salarioBase`.
     - `S.staff` queda con los de la API, en el orden de la API.
2. **`FORM.employee`** se engancha con `PitBossApi.conectar(FORM, 'employee', { antes, exito, guardar })`:
   - `antes(fd)`: devuelve `{ n: S.staff.length, documento: docApi(fd.get('doc')), nombre: fd.get('name').trim(), cargo: {Crupier:'CRUPIER', Cajero:'CAJERO', 'Jefe de piso':'JEFE_DE_PISO'}[fd.get('role')], juego: fd.get('game') }`
   - `exito: ctx => S.staff.length > ctx.n`
   - `guardar: ctx => pedir('POST', '/empleados', { documento, nombre, cargo, juego: cargo === 'CRUPIER' ? juego : null, tipoContrato: 'INDEFINIDO' })`
3. **Carga `nomina` (orden 80).** `obtener`: si `allowed('rrhh')`, llama a `pedir('GET', '/nomina')`; si no, `null`. `aplicar(lista)`: `S.nomina = lista.find(n => n.anio === 2026 && n.mes === 10) || null`.
4. **Reemplazar `payroll()`:** se guarda `const payrollLocal = payroll;` y se redefine `payroll = function () { ... }`. Cuando hay API y `S.nomina`, devuelve `S.nomina.detalle.map(d => ({ s: S.staff.find(x => x.apiDoc === d.empleadoDocumento) || { name: d.empleadoNombre, shift: {MANANA:'Mañana',TARDE:'Tarde',NOCHE:'Noche'}[d.turno] }, base: d.salarioBase, noct: d.recargoNocturno, aux: d.auxilioTransporte, ded: d.totalDeducciones, net: d.neto, contrato: d.tipoContrato }))`; si no, `payrollLocal()`. Con esto, la tarjeta "Nómina de octubre" de `V.rrhh` muestra los totales reales sin tocar `V.rrhh`.
5. **Calcular al abrir el modal:** `PitBossApi.conectar(ACT, 'modal', { antes: a => a === 'payroll' && (!S.nomina || S.nomina.estado === 'BORRADOR') ? {} : null, guardar: () => PitBossApi.pedir('POST', '/nomina/calculate', { anio: 2026, mes: 10 }) })`. El `sincronizar()` posterior recarga `S.nomina`.
6. **Reemplazar `MODALS.payroll`** cuando hay `S.nomina`. Se guarda `const modalLocal = MODALS.payroll`. La versión nueva (con los helpers `money`, `esc` y `B` del mockup) muestra:
   - título `Nómina de octubre · cálculo`;
   - subtítulo `HU-04 · Salario base, recargo nocturno del 35 %, auxilio de transporte (hasta 2 SMMLV) y aportes a salud y pensión (4 % + 4 %) o retención del 10 % en prestación de servicios. Parámetros 2026.`;
   - una tabla con las columnas `Empleado | Turno | Contrato | Base | Recargo | Auxilio | Deducciones | Neto` (contrato: `INDEFINIDO`→`Indefinido`, `TERMINO_FIJO`→`Término fijo`, `PRESTACION_SERVICIOS`→`Servicios`);
   - el pie con `Total neto` (`S.nomina.totalNeto`) y `Aportes del empleador (parafiscales)` (`S.nomina.totalAportesEmpleador`);
   - si `estado === 'APROBADA'`, `B('Aprobada','ok')` y sin botón de aprobar; si no, los botones `Cerrar` y `Aprobar nómina` (`data-act="approvePayroll"`).
   
   Sin API o sin `S.nomina`, se usa `modalLocal(m)`.
7. **`ACT.approvePayroll`:** `PitBossApi.conectar(ACT, 'approvePayroll', { antes: () => S.nomina && S.nomina.estado === 'BORRADOR' ? { id: S.nomina.id } : null, guardar: ({ id }) => PitBossApi.pedir('POST', `/nomina/${id}/aprobar`) })`.

- [ ] **F6.1** Implementar `personal.js` como una IIFE: `(function () { const { pedir, conectar, registrarCarga, util } = PitBossApi; ... })();`.
- [ ] **F6.2 Probar a mano** con `$env:BaseDatos__RecrearAlIniciar="true"; dotnet run --project src/CasinoErp.Api`, en http://localhost:5080, entrando como **Felipe Cortés** (Coordinador de talento humano):
  1. **Competencias → + Registrar empleado:** "Marta Gil", documento `1099888777`, Cajero → toast de registro; al recargar (F5) y volver a entrar, Marta sigue en la lista.
  2. Registrar otra vez el documento `1099888777` → toast `Ya existe un empleado con ese documento.`
  3. **Turnos y nómina → Calcular nómina:** el modal muestra 19 filas (18 de demo + Marta). La fila de Bruno Lara tiene recargo $910.000 y neto $3.229.200. La de Ana Torres tiene auxilio $249.095 y neto $2.641.095.
  4. **Aprobar nómina** → toast `Nómina aprobada. Se pagará el 30/10/2026.`; al abrir otra vez el modal aparece la etiqueta **Aprobada**, sin botón.
  5. Como **Ana Torres** (crupier), **Mi mesa** sigue mostrando su mesa M-01 (el `id` `s1` se conservó).
  6. Con la API apagada, el modal muestra el cálculo de ejemplo del mockup.
  
  Anotar cada resultado en la bitácora.
- [ ] **F6.3** Commit, push y PR: `HU-04: Front de empleados y nomina conectado a la API`.

---

### Tarea F7 · Autorización de tus controladores (después de la I5 de Iván)

Cuando Iván anuncie en su decisión D-0NN que "la seguridad está activa en develop":

- [ ] **F7.1** `git switch develop; git pull --rebase origin develop` y luego hacer rebase de tus ramas abiertas.
- [ ] **F7.2** Si Iván no lo hizo ya (se verifica con `grep -rn "AutorizarModulo" src/CasinoErp.Api/Controllers`), poner `[AutorizarModulo(Modulos.Competencias, Modulos.Rrhh, Modulos.Mesas)]` en `EmpleadosController` y `[AutorizarModulo(Modulos.Rrhh)]` en `NominaController` (contrato C5).
- [ ] **F7.3** En `PersonalNominaApiTests`, crear el cliente con `await _factory.ClienteComoAsync("felipe.cortes@pitboss.co")` y agregar:

```csharp
[Fact] public async Task Un_cajero_no_puede_calcular_la_nomina()
{
    var cajero = await _factory.ClienteComoAsync("laura.gomez@pitboss.co");
    (await cajero.PostAsJsonAsync("/api/nomina/calculate", new { anio = 2026, mes = 10 })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
}
[Fact] public async Task El_jefe_de_piso_lista_empleados_pero_no_ve_nomina()   // daniela.vargas: GET /api/empleados 200; GET /api/nomina 403
```

- [ ] **F7.4** Ejecutar `dotnet test CasinoErp.sln` (verde) y probar a mano que Laura Gómez no ve "Turnos y nómina". Commit: `HU-04: Autorizacion por modulo en empleados y nomina`.

---

### Tarea F8 · arc42 (bloque Nómina) y cierre

**Archivos:** `docs/arc42/05-bloques/nomina.md`, `docs/arc42/09-decisiones/ADR-005-parametros-nomina.md`.

- [ ] **F8.1** `05-bloques/nomina.md`:
  - **Propósito y frontera** del módulo: qué expone (los endpoints de F4 y F5) y de qué depende (solo `IEmpleadoRepositorio` y la persistencia).
  - Diagrama Mermaid `classDiagram` con `NominaService`, `PayrollEngine`, `IReglaLiquidacion`, `ReglaContratoLaboral`, `ReglaPrestacionServicios`, `ParametrosNomina`, `Nomina`, `LiquidacionEmpleado` y las tres interfaces de repositorio.
  - Una sección **"SOLID en el código"**: SRP (qué hace cada clase, con su ruta), DIP (las inyecciones, con el fragmento del constructor) y OCP (cómo se agrega un contrato nuevo).
  - El **diagrama de secuencia** Mermaid `sequenceDiagram` de `POST /api/nomina/calculate`: JefeRRHH → Front (`personal.js`) → `NominaController` (JWT, `[AutorizarModulo(Modulos.Rrhh)]`) → `NominaService.CalcularAsync` → `INominaRepositorio.ObtenerPorPeriodoAsync` (¿aprobada?) → `IParametrosNominaRepositorio.ObtenerAsync(2026)` → `IEmpleadoRepositorio.ListarAsync(true)` → `PayrollEngine.Calcular` → `loop` por empleado: `IReglaLiquidacion.Liquidar` → `EliminarAsync` del borrador anterior (opcional) → `AgregarAsync` → `Respuesta "Nómina calculada"`. Incluir un `alt` para las ramas de error (mes inválido, nómina aprobada, salario bajo el mínimo).
  - La tabla de reglas de liquidación con los casos de prueba de F2 como ejemplos numéricos.
- [ ] **F8.2** `ADR-005-parametros-nomina.md`: contexto (decreto 2026 en disputa judicial), decisión (parámetros por año editables por API), consecuencias y la simplificación del Art. 114-1 (de D-003).
- [ ] **F8.3** **Aporte a la retrospectiva:** en tu bitácora, una entrada "Retrospectiva HU-04" con (a) cuántas horas tomó realmente contra lo estimado, (b) qué casos de borde no se vieron en el planning y (c) tu propuesta para la regla de estimación del Sprint 3. Jorge lo consolida en `docs/sprint-2/retrospectiva.md`.
- [ ] **F8.4 Demo de la Sprint Review** (tu parte, máximo 2 minutos): login como Felipe → Turnos y nómina → Calcular → mostrar la fila de Bruno Lara (recargo nocturno sin auxilio) y la de Ana Torres (auxilio) → Aprobar. Ensayarla una vez y anotar el tiempo en la bitácora.
- [ ] **F8.5** Commit y PR: `HU-08: arc42 bloque Nomina con diagrama de secuencia y ADR-005`. Mover HU-04 a "Listo" en Jira. Escribir la entrada final de la bitácora: estado de cada tarea, pendientes y decisiones.

---

### Tarea F9 · Dar a Jorge acceso a la suscripción de Azure (decisión D-010)

**Por qué es tuya:** la suscripción de Azure donde están `rg-pitboss-erp` y `pitboss-erp-8szxa` es tuya (usuario de GitHub `Flashtwiice`) y solo quien es Owner de la suscripción puede asignar roles. Hoy Jorge (`jalcidesdiaz@ucundinamarca.edu.co`) solo tiene **Contributor sobre la web app `pitboss-erp-8szxa`**, y eso no alcanza para la HU-07 (Terraform). No es código: no lleva rama ni PR. Hazla **antes de F4**.

**Archivos:** ninguno. Solo el portal de Azure y tu bitácora.

- [ ] **F9.1** En portal.azure.com abre **Suscripciones** → la suscripción *Azure for Students* → **Control de acceso (IAM)** → **Agregar** → **Agregar asignación de roles**.
- [ ] **F9.2** Asigna el rol **Contributor** (pestaña *Roles de administrador con privilegios*) a `jalcidesdiaz@ucundinamarca.edu.co`, con alcance en la **suscripción completa**, no solo en la web app. Hace falta porque Jorge debe crear el grupo `rg-pitboss-tfstate` (J2) y leer e importar el plan de App Service, el servidor SQL, la base de datos y las reglas de firewall de `rg-pitboss-erp` (J3).
- [ ] **F9.3** Repite con el rol **User Access Administrator** (mismo correo, misma suscripción). Hace falta para que Jorge pueda asignarse `Storage Blob Data Contributor` sobre el state (J2.3) y crear los roles del workflow de GitHub con OIDC (J4.1). Si prefieres un solo paso, asigna **Owner** sobre la suscripción: cubre los dos roles.
- [ ] **F9.4** Verifica en **Control de acceso (IAM)** → **Asignaciones de roles** que Jorge aparece con los dos roles (o Owner) con alcance *Esta suscripción*.
- [ ] **F9.5** Avísale a Jorge (chat del grupo) y anota en tu bitácora: fecha y hora, roles asignados y alcance. Jorge comprueba con `az role assignment list` y retoma J2.
- [ ] **F9.6** *(Para J4, cuando Jorge lo pida)* Cargar en el repositorio de GitHub los secretos `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `TFSTATE_RG` y `TFSTATE_SA` (solo quien administra el repo puede hacerlo). Jorge te pasa los valores cuando termine J4.1.

**Fuera de alcance:** no instales ni ejecutes Terraform; eso es de Jorge. No cambies nada más de la suscripción.
