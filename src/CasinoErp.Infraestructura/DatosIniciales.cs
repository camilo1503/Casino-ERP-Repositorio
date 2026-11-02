using CasinoErp.Dominio.Caja;
using CasinoErp.Dominio.Inventario;
using CasinoErp.Dominio.Jugadores;
using CasinoErp.Dominio.Mesas;
using CasinoErp.Dominio.Rellenos;
using CasinoErp.Dominio.Retencion;

namespace CasinoErp.Infraestructura;

/// <summary>Datos de demostración: los mismos del prototipo del front (wwwroot/index.html).</summary>
public static class DatosIniciales
{
    public static void Cargar(CasinoErpDbContext db)
    {
        if (db.Jugadores.Any()) return;

        var hoy = DateTime.Today;
        DateTime A(int dias, int hora, int minuto) => hoy.AddDays(dias).AddHours(hora).AddMinutes(minuto).ToUniversalTime();

        Jugador J(string doc, string nombre, string correo, string tel, string ciudad, string nacimiento, int puntos,
            string prefs, string franja, long apuesta) => new()
        {
            Documento = doc, Nombre = nombre, Correo = correo, Telefono = tel, Ciudad = ciudad,
            FechaNacimiento = DateOnly.Parse(nacimiento), Puntos = puntos, Preferencias = [.. prefs.Split(',')],
            Franja = franja, ApuestaPromedio = apuesta, FechaRegistro = A(-200, 12, 0),
        };

        var diego = J("80554219", "Diego Martínez", "diego.martinez@correo.com", "+57 318 120 7745", "Bogotá", "1988-04-17", 2700, "Blackjack", "Noche", 90_000);
        diego.Estado = EstadoJugador.Autoexcluido;
        diego.MotivoEstado = "Solicitud del jugador por juego responsable";

        db.Jugadores.AddRange(
            J("1020456789", "María Fernanda López", "maria.lopez@correo.com", "+57 310 555 0142", "Bogotá", "1990-05-14", 8450, "Blackjack,Ruleta,Póker", "Noche", 250_000),
            J("1018334560", "Andrés Castro", "andres.castro@correo.com", "+57 315 220 1188", "Chía", "1985-11-02", 3200, "Ruleta,Blackjack", "Tarde", 120_000),
            J("52889301", "Juliana Reyes", "juliana.reyes@correo.com", "+57 320 448 9021", "Bogotá", "1979-02-21", 15200, "Baccarat,Blackjack", "Noche", 600_000),
            J("79456112", "Carlos Pineda", "carlos.pineda@correo.com", "+57 301 667 4410", "Zipaquirá", "1982-07-09", 9100, "Póker,Blackjack", "Noche", 300_000),
            J("1032778410", "Sofía Herrera", "sofia.herrera@correo.com", "+57 312 900 3371", "Cajicá", "1996-09-30", 900, "Tragamonedas", "Tarde", 50_000),
            diego,
            J("1026441093", "Valentina Rojas", "valentina.rojas@correo.com", "+57 311 554 2290", "Chía", "1999-12-05", 1300, "Ruleta,Tragamonedas", "Mañana", 40_000),
            J("80112774", "Luis Ortega", "luis.ortega@correo.com", "+57 317 330 6612", "Sopó", "1975-01-28", 7800, "Blackjack,Craps", "Noche", 280_000));

        Mesa M(string numero, string juego, EstadoMesa estado, int asientos, long min, long max, string zona = "Zona A") =>
            new() { Numero = numero, Juego = juego, Estado = estado, Asientos = asientos, ApuestaMinima = min, ApuestaMaxima = max, Zona = zona };
        const EstadoMesa a = EstadoMesa.Abierta, c = EstadoMesa.Cerrada, m = EstadoMesa.Mantenimiento;
        db.Mesas.AddRange(
            M("M-01", "Blackjack", a, 7, 25_000, 500_000), M("M-02", "Ruleta", a, 8, 10_000, 200_000), M("M-03", "Póker", a, 9, 100_000, 2_000_000),
            M("M-04", "Baccarat", a, 7, 50_000, 1_000_000), M("M-05", "Blackjack", c, 7, 50_000, 1_000_000), M("M-06", "Craps", a, 10, 10_000, 200_000),
            M("M-07", "Ruleta", a, 8, 25_000, 500_000), M("M-08", "Blackjack", a, 7, 10_000, 200_000), M("M-09", "Póker", a, 9, 50_000, 1_000_000),
            M("M-10", "Baccarat", m, 7, 100_000, 2_000_000), M("M-11", "Ruleta", a, 8, 10_000, 200_000), M("M-12", "Blackjack", a, 7, 25_000, 500_000),
            M("M-13", "Póker", c, 9, 50_000, 1_000_000), M("M-14", "Blackjack", a, 7, 10_000, 200_000), M("M-15", "Ruleta", a, 8, 25_000, 500_000),
            M("M-16", "Baccarat", a, 7, 50_000, 1_000_000), M("M-17", "Craps", c, 10, 10_000, 200_000), M("M-18", "Blackjack", a, 7, 10_000, 200_000),
            M("TR-1", "Tragamonedas", a, 20, 1_000, 50_000, "Zona B"));

        db.Rellenos.AddRange(
            new Relleno { MesaNumero = "M-12", Monto = 10_000_000, SolicitadoPor = "Luis Méndez", SolicitadoEn = A(0, 20, 41) },
            new Relleno { MesaNumero = "M-04", Monto = 5_000_000, SolicitadoPor = "Camila Pardo", SolicitadoEn = A(0, 20, 58) });

        TransaccionCaja T(TipoTransaccion tipo, string? doc, long monto, DateTime fecha, string ubicacion,
            EstadoTransaccion estado = EstadoTransaccion.Aprobada, string caja = "Caja 2", string? cumplimiento = null) =>
            new() { Tipo = tipo, Documento = doc, Monto = monto, Fecha = fecha, Ubicacion = ubicacion, Estado = estado, Caja = caja, EstadoCumplimiento = cumplimiento };
        const TipoTransaccion compra = TipoTransaccion.Compra, pago = TipoTransaccion.Pago;
        db.Transacciones.AddRange(
            T(compra, "1020456789", 1_000_000, A(-14, 21, 5), "Póker · M-03", caja: "Caja 1"),
            T(pago, "1020456789", 1_800_000, A(-7, 22, 41), "Ruleta · M-02", caja: "Caja 1"),
            T(pago, "1018334560", 7_200_000, A(-1, 23, 48), "Ruleta · M-02", caja: "Caja 1", cumplimiento: "Reportada"),
            T(compra, null, 5_400_000, A(0, 13, 5), "Ruleta · M-07", caja: "Caja 1", cumplimiento: "Escalada"),
            T(compra, "80112774", 6_500_000, A(0, 15, 12), "Blackjack · M-14", caja: "Caja 1", cumplimiento: "En revisión"),
            T(compra, "52889301", 12_000_000, A(0, 16, 40), "Baccarat · M-04", caja: "Caja 1", cumplimiento: "Reportada"),
            T(compra, "80112774", 4_000_000, A(0, 17, 31), "Craps · M-06"),
            T(pago, "1026441093", 650_000, A(0, 17, 40), "Ruleta · M-11"),
            T(compra, "1026441093", 300_000, A(0, 17, 48), "Ruleta · M-11", EstadoTransaccion.Anulada),
            T(compra, "1032778410", 1_000_000, A(0, 17, 55), "Tragamonedas · TR-1"),
            T(pago, "79456112", 8_750_000, A(0, 18, 2), "Póker · M-03", EstadoTransaccion.EnRevision, cumplimiento: "En revisión"),
            T(compra, "52889301", 500_000, A(0, 18, 11), "Baccarat · M-04"),
            T(pago, "1018334560", 1_200_000, A(0, 18, 19), "Ruleta · M-02"),
            T(compra, "1020456789", 2_500_000, A(0, 18, 24), "Blackjack · M-01"));

        var fecha = DateOnly.FromDateTime(hoy);
        db.Ofertas.AddRange(
            new OfertaRetencion { Documento = "1018334560", Senal = "Solicitud de cobro en caja", Origen = "Caja 2", Resultado = ResultadoOferta.Rechazada, Fecha = fecha, RegistradaEn = A(0, 18, 55) },
            new OfertaRetencion { Documento = "52889301", Senal = "Cambio a fichas grandes", Origen = "Mesa M-04", Resultado = ResultadoOferta.Aceptada, Tipo = TipoOferta.BonoReclamable, Valor = 100_000, Fecha = fecha, RegistradaEn = A(0, 19, 42), Vence = fecha.AddDays(30) });
        db.ConfiguracionRetencion.Add(new ConfiguracionRetencion());

        db.Fichas.AddRange(
            new Ficha { Codigo = "FICHA-5K", Denominacion = 5_000, Categoria = "Roja", Stock = 4200 },
            new Ficha { Codigo = "FICHA-10K", Denominacion = 10_000, Categoria = "Azul", Stock = 3800 },
            new Ficha { Codigo = "FICHA-25K", Denominacion = 25_000, Categoria = "Verde", Stock = 3100 },
            new Ficha { Codigo = "FICHA-50K", Denominacion = 50_000, Categoria = "Naranja", Stock = 2600 },
            new Ficha { Codigo = "FICHA-100K", Denominacion = 100_000, Categoria = "Negra", Stock = 1900 },
            new Ficha { Codigo = "FICHA-500K", Denominacion = 500_000, Categoria = "Morada", Stock = 420 });

        db.Bovedas.Add(new Boveda { Id = 1, Saldo = 150_000_000 });
        db.SaveChanges();
    }
}
