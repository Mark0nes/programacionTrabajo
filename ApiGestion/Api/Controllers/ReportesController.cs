using GestionEventos.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class ReportesController : BaseApiController
{
    private readonly EventoService _eventoService;
    private readonly UsuarioService _usuarioService;

    public ReportesController(EventoService eventoService, UsuarioService usuarioService)
    {
        _eventoService = eventoService;
        _usuarioService = usuarioService;
    }

    [HttpGet("recaudacion")]
    public IActionResult Recaudacion()
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var reporte = _eventoService.ObtenerReporteRecaudacion();
            return Ok(reporte);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }
}
