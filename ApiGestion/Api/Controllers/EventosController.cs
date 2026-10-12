using GestionEventos.Api.DTOs;
using GestionEventos.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class EventosController : BaseApiController
{
    private readonly EventoService _eventoService;

    public EventosController(EventoService eventoService)
    {
        _eventoService = eventoService;
    }

    /// <summary>
    /// Catálogo. Por defecto lista solo lo que se puede comprar (eventos no cancelados y que todavía no
    /// ocurrieron). Con ?incluirNoDisponibles=true también devuelve los cancelados y los ya pasados.
    /// </summary>
    [HttpGet]
    public IActionResult ObtenerEventos([FromQuery] bool incluirNoDisponibles = false)
    {
        return Ejecutar(() =>
        {
            var eventos = incluirNoDisponibles ? _eventoService.ObtenerParaGestion(ObtenerDni()) : _eventoService.ObtenerDisponibles();
            return Ok(eventos.Select(e => e.AResponse()).ToList());
        });
    }

    [HttpGet("gestion")]
    public IActionResult ObtenerParaGestion()
    {
        return Ejecutar(() => Ok(_eventoService.ObtenerParaGestion(ObtenerDni()).Select(e => e.AResponse()).ToList()));
    }

    /// <summary>Detalle con sus modalidades. Existe para cualquier evento, también cancelado o pasado.</summary>
    [HttpGet("{id:guid}")]
    public IActionResult ObtenerPorId(Guid id)
    {
        return Ejecutar(() => Ok(_eventoService.ObtenerPorId(id).AResponse()));
    }

    [HttpPost]
    public IActionResult Crear([FromBody] EventoRequest request)
    {
        return Ejecutar(() =>
        {
            var evento = _eventoService.Crear(ObtenerDni(), request.Nombre, request.Descripcion ?? string.Empty,
                request.Fecha!.Value, request.Lugar, request.Latitud, request.Longitud);
            return Created($"/api/eventos/{evento.Id}", evento.AResponse());
        });
    }

    [HttpPut("{id:guid}")]
    public IActionResult Modificar(Guid id, [FromBody] EventoRequest request)
    {
        return Ejecutar(() =>
        {
            var evento = _eventoService.Modificar(ObtenerDni(), id, request.Nombre, request.Descripcion ?? string.Empty,
                request.Fecha!.Value, request.Lugar, request.Latitud, request.Longitud);
            return Ok(evento.AResponse());
        });
    }

    [HttpPut("{id:guid}/cancelar")]
    public IActionResult Cancelar(Guid id)
    {
        return Ejecutar(() =>
        {
            var evento = _eventoService.Cancelar(ObtenerDni(), id);
            return Ok(new AccionResponse<EventoResponse>("Evento cancelado exitosamente.", evento.AResponse()));
        });
    }

    [HttpPost("{id:guid}/modalidades")]
    public IActionResult AgregarModalidad(Guid id, [FromBody] ModalidadRequest request)
    {
        return Ejecutar(() =>
        {
            var modalidad = _eventoService.AgregarModalidad(ObtenerDni(), id, request.Nombre, request.Precio,
                request.Beneficios ?? string.Empty, request.CupoMaximo);
            return Created($"/api/eventos/{id}/modalidades/{modalidad.Id}", modalidad.AResponse());
        });
    }

    [HttpPut("{idEvento:guid}/modalidades/{idModalidad:guid}/cancelar")]
    public IActionResult CancelarModalidad(Guid idEvento, Guid idModalidad)
    {
        return Ejecutar(() =>
        {
            var modalidad = _eventoService.CancelarModalidad(ObtenerDni(), idEvento, idModalidad);
            return Ok(new AccionResponse<ModalidadResponse>("Modalidad cancelada exitosamente.", modalidad.AResponse()));
        });
    }
}
