using GestionEventos.Logica;
using GestionEventos.Api.DTOs;
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

            var respuesta = new ReporteRecaudacionDto(
                reporte.TotalGeneralRecaudado,
                reporte.TotalGeneralEntradasVendidas,
                reporte.Eventos.Select(evento => new ReporteEventoDto(
                    evento.IdEvento,
                    evento.NombreEvento,
                    evento.Fecha,
                    evento.Lugar,
                    evento.Cancelado,
                    evento.EntradasVendidas,
                    evento.RecaudacionTotal,
                    evento.Modalidades.Select(modalidad => new ReporteModalidadDto(
                        modalidad.IdModalidad,
                        modalidad.NombreModalidad,
                        modalidad.PrecioUnitario,
                        modalidad.CupoMaximo,
                        modalidad.CupoDisponible,
                        modalidad.EntradasVendidas,
                        modalidad.Recaudacion,
                        modalidad.Cancelada)).ToList(),
                    evento.Compradores.Select(comprador => new ReporteCompradorDto(
                        comprador.Dni,
                        comprador.Nombre,
                        comprador.CantidadEntradas,
                        comprador.Total)).ToList())).ToList());

            return Ok(respuesta);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
    }
}
