using GestionEventos.Api.DTOs;
using GestionEventos.Logica.Excepciones;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// ÚNICO criterio para saber quién opera: header "X-Dni" (o, como alternativa para pruebas
    /// rápidas desde el navegador, query string "?dni="). Ningún endpoint lo toma del body.
    /// </summary>
    protected string? ObtenerDni()
    {
        if (Request.Headers.TryGetValue("X-Dni", out var headerDni) && !string.IsNullOrWhiteSpace(headerDni))
        {
            return headerDni.ToString().Trim();
        }

        if (Request.Query.TryGetValue("dni", out var queryDni) && !string.IsNullOrWhiteSpace(queryDni))
        {
            return queryDni.ToString().Trim();
        }

        return null;
    }

    /// <summary>
    /// Ejecuta la acción del endpoint y traduce las excepciones del dominio a códigos HTTP,
    /// siempre con el motivo en el body: 403 acceso denegado, 404 no existe, 400 regla de negocio.
    /// El Controller no decide reglas: solo traduce.
    /// </summary>
    protected IActionResult Ejecutar(Func<IActionResult> accion)
    {
        try
        {
            return accion();
        }
        catch (AccesoDenegadoException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new ErrorResponse(ex.Message));
        }
        catch (EntidadNoEncontradaException ex)
        {
            return NotFound(new ErrorResponse(ex.Message));
        }
        catch (ReglaNegocioException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse("Error inesperado: " + ex.Message));
        }
    }
}
