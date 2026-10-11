using GestionEventos.Api.DTOs;
using GestionEventos.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class ReportesController : BaseApiController
{
    private readonly EventoService _eventoService;

    public ReportesController(EventoService eventoService)
    {
        _eventoService = eventoService;
    }

    [HttpGet("recaudacion")]
    public IActionResult Recaudacion()
    {
        return Ejecutar(() => Ok(_eventoService.ObtenerReporteRecaudacion(ObtenerDni()).AResponse()));
    }
}
