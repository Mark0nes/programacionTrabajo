using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected string? ObtenerDni()
    {
        if (Request.Headers.TryGetValue("X-Dni", out var headerDni) && !string.IsNullOrWhiteSpace(headerDni))
        {
            return headerDni.ToString();
        }
        if (Request.Query.TryGetValue("dni", out var queryDni) && !string.IsNullOrWhiteSpace(queryDni))
        {
            return queryDni.ToString();
        }
        return null;
    }
}
