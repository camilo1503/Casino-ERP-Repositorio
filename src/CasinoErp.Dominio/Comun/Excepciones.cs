namespace CasinoErp.Dominio.Comun;

/// <summary>Regla de negocio incumplida. El mensaje es el que ve el usuario en el ERP.</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}

/// <summary>El registro solicitado no existe.</summary>
public class NoEncontradoException : Exception
{
    public NoEncontradoException(string mensaje) : base(mensaje) { }
}

/// <summary>Un retiro supera el stock disponible (Taller 4).</summary>
public class StockInsuficienteException : ReglaNegocioException
{
    public StockInsuficienteException() : base("Stock insuficiente") { }
}
