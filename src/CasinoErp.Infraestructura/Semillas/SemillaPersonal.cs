using CasinoErp.Dominio.Personal;

namespace CasinoErp.Infraestructura.Semillas;

/// <summary>
/// Los 18 empleados de seed().staff del mockup, en el mismo orden. El empleado i (desde 0) tiene
/// Documento = 1010200300 + i * 7919 (tabla de desarrollo/contratos.md §C4).
/// </summary>
public class SemillaPersonal : ISemilla
{
    public int Orden => 10;

    private static readonly (string Nombre, Cargo Cargo, string Juego, Turno Turno, string Area)[] Staff =
    [
        ("Ana Torres", Cargo.Crupier, "Blackjack", Turno.Tarde, "M-01"),
        ("Pedro Ruiz", Cargo.Crupier, "Blackjack", Turno.Tarde, "Sin asignar"),
        ("Laura Gómez", Cargo.Cajero, "Caja", Turno.Tarde, "Caja 2"),
        ("Miguel Á. Soto", Cargo.Crupier, "Ruleta", Turno.Tarde, "M-02"),
        ("Santiago Mora", Cargo.Crupier, "Póker", Turno.Tarde, "M-03"),
        ("Camila Pardo", Cargo.Crupier, "Baccarat", Turno.Tarde, "M-04"),
        ("Julián Ríos", Cargo.Crupier, "Craps", Turno.Tarde, "M-06"),
        ("Óscar Díaz", Cargo.Crupier, "Ruleta", Turno.Tarde, "M-07"),
        ("Natalia Gil", Cargo.Crupier, "Blackjack", Turno.Tarde, "M-08"),
        ("Felipe Rojas", Cargo.Crupier, "Póker", Turno.Tarde, "M-09"),
        ("Paula Ríos", Cargo.Crupier, "Ruleta", Turno.Tarde, "M-11"),
        ("Luis Méndez", Cargo.Crupier, "Blackjack", Turno.Tarde, "M-12"),
        ("Andrea Ruiz", Cargo.Crupier, "Blackjack", Turno.Tarde, "M-14"),
        ("Carlos Vega", Cargo.Crupier, "Ruleta", Turno.Tarde, "M-15"),
        ("Diana Soler", Cargo.Crupier, "Baccarat", Turno.Tarde, "M-16"),
        ("Tomás Gil", Cargo.Crupier, "Blackjack", Turno.Tarde, "M-18"),
        ("Daniela Vargas", Cargo.JefeDePiso, "Supervisión", Turno.Tarde, "Piso A"),
        ("Bruno Lara", Cargo.Crupier, "Póker", Turno.Noche, "Sin asignar"),
    ];

    public void Cargar(CasinoErpDbContext db)
    {
        if (db.Set<Empleado>().Any()) return;

        db.Set<Empleado>().AddRange(Staff.Select((s, i) => new Empleado
        {
            Documento = (1010200300 + i * 7919).ToString(),
            Nombre = s.Nombre,
            Cargo = s.Cargo,
            Juego = s.Juego,
            Turno = s.Turno,
            Area = s.Area,
            SalarioBase = EmpleadoService.SalarioPorCargo[s.Cargo],
            TipoContrato = TipoContrato.Indefinido,
            FechaIngreso = new DateOnly(2025, 1, 15),
            Activo = true,
        }));
        db.SaveChanges();
    }
}
