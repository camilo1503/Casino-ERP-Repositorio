using CasinoErp.Dominio.Comun;
using CasinoErp.Dominio.Compras;
using CasinoErp.Dominio.Inventario;
using CasinoErp.PruebasUnitarias.Dobles;
using FluentAssertions;

namespace CasinoErp.PruebasUnitarias;

/// <summary>HU-05 · Generación automática y ciclo de vida de las órdenes de compra (Plan-Camilo C4).</summary>
public class OrdenesCompraTests
{
    private static readonly DateTime Ahora = new(2026, 10, 26, 15, 0, 0, DateTimeKind.Utc);

    private readonly FichasEnMemoria _fichas = new();
    private readonly AlertasEnMemoria _alertas = new();
    private readonly ProveedoresEnMemoria _proveedores = new();
    private readonly OrdenesEnMemoria _ordenes = new();
    private readonly GeneradorOrdenesCompra _generador;
    private readonly InventarioFichasService _inventario;
    private readonly OrdenCompraService _servicio;

    public OrdenesCompraTests()
    {
        var reloj = new RelojFijo();
        _generador = new GeneradorOrdenesCompra(_ordenes, _proveedores, reloj);
        _inventario = new InventarioFichasService(_fichas, _alertas, [_generador], reloj);
        _servicio = new OrdenCompraService(_ordenes, _proveedores, _inventario, reloj);
        _fichas.Datos.Add(new Ficha { Codigo = "FICHA-500K", Denominacion = 500_000, Categoria = "Morada", Stock = 420, StockMinimo = 400 });
        _fichas.Datos.Add(new Ficha { Codigo = "FICHA-25K", Denominacion = 25_000, Categoria = "Verde", Stock = 3100 });
    }

    private static Proveedor P(string nit, bool activo = true) =>
        new() { Nit = nit, Nombre = "Proveedor " + nit, Correo = "p@p.co", Telefono = "1", Activo = activo };

    private Ficha Ficha500K => _fichas.Datos.Single(f => f.Codigo == "FICHA-500K");

    private static AlertaStock Alerta500K() => new() { CodigoFicha = "FICHA-500K", StockActual = 395, StockMinimo = 400 };

    private async Task<OrdenCompra> BorradorManual(string? nit = "901234567-1") =>
        await _servicio.CrearManualAsync("FICHA-25K", 1500, nit);

    [Fact]
    public async Task La_alerta_genera_un_borrador_para_el_primer_proveedor_activo()
    {
        _proveedores.Datos.AddRange([P("900765432-8", activo: false), P("901234567-1")]);

        await _generador.StockCriticoAsync(Alerta500K(), Ficha500K);

        var o = (await _ordenes.ListarAsync()).Single();
        o.Cantidad.Should().Be(165);                 // ceil(400 * 1.4) - 395
        o.Estado.Should().Be(EstadoOrden.Borrador);
        o.Origen.Should().Be("Alerta de stock");
        o.ProveedorNit.Should().Be("901234567-1");
        o.CodigoFicha.Should().Be("FICHA-500K");
        o.CreadaEn.Should().Be(Ahora);
    }

    [Fact]
    public async Task No_crea_otra_orden_si_ya_hay_una_abierta_para_la_ficha()
    {
        _proveedores.Datos.Add(P("901234567-1"));
        await _generador.StockCriticoAsync(Alerta500K(), Ficha500K);

        await _generador.StockCriticoAsync(Alerta500K(), Ficha500K);

        _ordenes.Datos.Should().ContainSingle();
    }

    [Fact]
    public async Task Si_la_orden_anterior_se_recibio_una_nueva_alerta_crea_otra()
    {
        _proveedores.Datos.Add(P("901234567-1"));
        await _generador.StockCriticoAsync(Alerta500K(), Ficha500K);
        _ordenes.Datos[0].Estado = EstadoOrden.Recibida;

        await _generador.StockCriticoAsync(Alerta500K(), Ficha500K);

        _ordenes.Datos.Should().HaveCount(2);
    }

    [Fact]
    public async Task Sin_proveedores_activos_el_borrador_queda_sin_proveedor()
    {
        _proveedores.Datos.Add(P("900765432-8", activo: false));

        await _generador.StockCriticoAsync(Alerta500K(), Ficha500K);

        _ordenes.Datos.Single().ProveedorNit.Should().BeNull();
    }

    [Fact]
    public async Task La_cantidad_minima_es_una_unidad()
    {
        var alerta = new AlertaStock { CodigoFicha = "FICHA-500K", StockActual = 0, StockMinimo = 0 };

        await _generador.StockCriticoAsync(alerta, Ficha500K);

        _ordenes.Datos.Single().Cantidad.Should().Be(1);
    }

    [Fact]
    public async Task Flujo_completo_retiro_alerta_orden_recepcion()
    {
        _proveedores.Datos.Add(P("901234567-1"));

        await _inventario.RetirarAsync("FICHA-500K", 25);
        var borrador = (await _servicio.ListarAsync()).Single();
        borrador.Estado.Should().Be(EstadoOrden.Borrador);
        borrador.Cantidad.Should().Be(165);

        await _servicio.EnviarAsync(borrador.Id, null);
        var recibida = await _servicio.RecibirAsync(borrador.Id);

        recibida.Estado.Should().Be(EstadoOrden.Recibida);
        recibida.ActualizadaEn.Should().Be(Ahora);
        Ficha500K.Stock.Should().Be(560);
        (await _inventario.ListarAlertasAsync()).Should().BeEmpty();
        (await _inventario.ListarAlertasAsync(false)).Single().Estado.Should().Be(EstadoAlerta.Atendida);
    }

    [Fact]
    public async Task Crea_orden_manual()
    {
        _proveedores.Datos.Add(P("901234567-1"));

        var o = await BorradorManual();

        o.Origen.Should().Be("Manual");
        o.Estado.Should().Be(EstadoOrden.Borrador);
        o.Cantidad.Should().Be(1500);
        o.ProveedorNit.Should().Be("901234567-1");
        o.CreadaEn.Should().Be(Ahora);
    }

    [Fact]
    public async Task Orden_manual_sin_proveedor()
    {
        var o = await BorradorManual(nit: null);

        o.ProveedorNit.Should().BeNull();
    }

    [Fact]
    public async Task Orden_manual_valida_cantidad()
    {
        var accion = () => _servicio.CrearManualAsync("FICHA-25K", 0, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La cantidad debe ser mayor que cero");
    }

    [Fact]
    public async Task Orden_manual_con_ficha_inexistente()
    {
        var accion = () => _servicio.CrearManualAsync("FICHA-9K", 10, null);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Ficha no encontrada");
    }

    [Fact]
    public async Task Orden_manual_con_proveedor_inexistente()
    {
        var accion = () => BorradorManual("999999999-9");

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Proveedor no encontrado");
    }

    [Fact]
    public async Task Orden_manual_con_proveedor_inactivo()
    {
        _proveedores.Datos.Add(P("901234567-1", activo: false));

        var accion = () => BorradorManual();

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El proveedor está inactivo.");
    }

    [Fact]
    public async Task Enviar_exige_proveedor()
    {
        var o = await BorradorManual(nit: null);

        var accion = () => _servicio.EnviarAsync(o.Id, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("Asigna un proveedor antes de enviar la orden.");
    }

    [Fact]
    public async Task Enviar_asigna_el_proveedor_recibido()
    {
        _proveedores.Datos.Add(P("900765432-8"));
        var o = await BorradorManual(nit: null);

        var enviada = await _servicio.EnviarAsync(o.Id, "900765432-8");

        enviada.Estado.Should().Be(EstadoOrden.Enviada);
        enviada.ProveedorNit.Should().Be("900765432-8");
        enviada.ActualizadaEn.Should().Be(Ahora);
    }

    [Fact]
    public async Task Enviar_rechaza_proveedor_inactivo()
    {
        _proveedores.Datos.Add(P("900765432-8", activo: false));
        var o = await BorradorManual(nit: null);

        var accion = () => _servicio.EnviarAsync(o.Id, "900765432-8");

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("El proveedor está inactivo.");
    }

    [Fact]
    public async Task No_envia_dos_veces()
    {
        _proveedores.Datos.Add(P("901234567-1"));
        var o = await BorradorManual();
        await _servicio.EnviarAsync(o.Id, null);

        var accion = () => _servicio.EnviarAsync(o.Id, null);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La orden no está en un estado válido para esta acción.");
    }

    [Fact]
    public async Task No_recibe_un_borrador()
    {
        var o = await BorradorManual(nit: null);

        var accion = () => _servicio.RecibirAsync(o.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La orden no está en un estado válido para esta acción.");
    }

    [Fact]
    public async Task No_cancela_una_recibida()
    {
        _proveedores.Datos.Add(P("901234567-1"));
        var o = await BorradorManual();
        await _servicio.EnviarAsync(o.Id, null);
        await _servicio.RecibirAsync(o.Id);

        var accion = () => _servicio.CancelarAsync(o.Id);

        await accion.Should().ThrowAsync<ReglaNegocioException>().WithMessage("La orden no está en un estado válido para esta acción.");
    }

    [Fact]
    public async Task Cancela_un_borrador()
    {
        var o = await BorradorManual(nit: null);

        var cancelada = await _servicio.CancelarAsync(o.Id);

        cancelada.Estado.Should().Be(EstadoOrden.Cancelada);
        cancelada.ActualizadaEn.Should().Be(Ahora);
    }

    [Fact]
    public async Task Cancela_una_enviada()
    {
        _proveedores.Datos.Add(P("901234567-1"));
        var o = await BorradorManual();
        await _servicio.EnviarAsync(o.Id, null);

        (await _servicio.CancelarAsync(o.Id)).Estado.Should().Be(EstadoOrden.Cancelada);
    }

    [Fact]
    public async Task Orden_inexistente_lanza_no_encontrado()
    {
        var accion = () => _servicio.EnviarAsync(999, null);

        await accion.Should().ThrowAsync<NoEncontradoException>().WithMessage("Orden de compra no encontrada");
    }

    [Fact]
    public void El_codigo_tiene_cuatro_digitos()
    {
        new OrdenCompra { Id = 7 }.Codigo.Should().Be("OC-0007");
    }
}
