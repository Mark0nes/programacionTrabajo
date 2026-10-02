using GestionEventos.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class EntradasController : BaseApiController
{
    private readonly CompraService _compraService;
    private readonly UsuarioService _usuarioService;

    public EntradasController(CompraService compraService, UsuarioService usuarioService)
    {
        _compraService = compraService;
        _usuarioService = usuarioService;
    }

    [HttpDelete("{codigo}")]
    public IActionResult CancelarEntrada(string codigo)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Comprador);

            var entradaCancelada = _compraService.CancelarEntrada(codigo, dni!);
            return Ok(new { mensaje = "Entrada cancelada con éxito. El cupo ha sido restablecido.", entrada = entradaCancelada });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
