using CasinoErp.Dominio.Comun;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CasinoErp.Api;

/// <summary>Convierte las excepciones del dominio en respuestas HTTP con el mensaje para el usuario.</summary>
public class FiltroExcepciones : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var estado = context.Exception switch
        {
            NoEncontradoException => StatusCodes.Status404NotFound,
            ReglaNegocioException => StatusCodes.Status400BadRequest,
            _ => (int?)null,
        };
        if (estado is null) return;

        context.Result = new ObjectResult(new { mensaje = context.Exception.Message }) { StatusCode = estado };
        context.ExceptionHandled = true;
    }
}

/// <summary>Respuesta estándar: mensaje para el usuario y el registro afectado.</summary>
public record Respuesta<T>(string Mensaje, T Datos);
