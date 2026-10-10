using GestionEventos.Logica;
using GestionEventos.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class EventosController : BaseApiController
{
    private readonly EventoService _eventoService;
    private readonly UsuarioService _usuarioService;

    public EventosController(EventoService eventoService, UsuarioService usuarioService)
    {
        _eventoService = eventoService;
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public IActionResult ObtenerEventos()
    {
        var eventos = _eventoService.ObtenerDisponibles();
        var eventoDtos = eventos.Select(evento => new EventoDto(
            evento.Nombre,
            evento.Descripcion,
            evento.Fecha,
            evento.Lugar,
            evento.Latitud,
            evento.Longitud
        )).ToList();
        return Ok(eventoDtos);
    }

    [HttpGet("{id:guid}")]
    public IActionResult ObtenerPorId(Guid id)
    {
        var evento = _eventoService.ObtenerPorId(id);
        var eventoDto = evento != null ? new EventoDto(
            evento.Nombre,
            evento.Descripcion,
            evento.Fecha,
            evento.Lugar,
            evento.Latitud,
            evento.Longitud
        ) : null;
        return evento != null
            ? Ok(eventoDto)
            : NotFound(new { mensaje = $"Evento con ID '{id}' no encontrado." });
    }

    [HttpPost]
    public IActionResult Crear([FromBody] EventoDto dto)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var nuevoEvento = _eventoService.Crear(dto.Nombre, dto.Descripcion, dto.Fecha, dto.Lugar, dto.Latitud, dto.Longitud);
            var eventoDto = new EventoDto(
                nuevoEvento.Nombre,
                nuevoEvento.Descripcion,
                nuevoEvento.Fecha,
                nuevoEvento.Lugar,
                nuevoEvento.Latitud,
                nuevoEvento.Longitud
            );
            return Created($"/api/eventos/{nuevoEvento.Id}", eventoDto);
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public IActionResult Modificar(Guid id, [FromBody] EventoDto dto)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var eventoModificado = _eventoService.Modificar(id, dto.Nombre, dto.Descripcion, dto.Fecha, dto.Lugar, dto.Latitud, dto.Longitud);
            var eventoDto = new EventoDto(
                eventoModificado.Nombre,
                eventoModificado.Descripcion,
                eventoModificado.Fecha,
                eventoModificado.Lugar,
                eventoModificado.Latitud,
                eventoModificado.Longitud
            );
            return Ok(eventoDto);
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

    [HttpPut("{id:guid}/cancelar")]
    public IActionResult Cancelar(Guid id)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var eventoCancelado = _eventoService.Cancelar(id);
            var eventoDto = new EventoDto(
                eventoCancelado.Nombre,
                eventoCancelado.Descripcion,
                eventoCancelado.Fecha,
                eventoCancelado.Lugar,
                eventoCancelado.Latitud,
                eventoCancelado.Longitud
            );
            return Ok(new { mensaje = "Evento cancelado exitosamente.", evento = eventoDto });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/modalidades")]
    public IActionResult AgregarModalidad(Guid id, [FromBody] ModalidadDto dto)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var modalidad = _eventoService.AgregarModalidad(id, dto.Nombre, dto.Precio, dto.Beneficios, dto.CupoMaximo);
            var modalidadDto = new ModalidadDto(
                modalidad.Nombre,
                modalidad.Precio,
                modalidad.Beneficios,
                modalidad.CupoMaximo
            );
            return Created($"/api/eventos/{id}/modalidades/{modalidad.Id}", modalidadDto);
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

    [HttpPut("{idEvento:guid}/modalidades/{idModalidad:guid}/cancelar")]
    public IActionResult CancelarModalidad(Guid idEvento, Guid idModalidad)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var modalidadCancelada = _eventoService.CancelarModalidad(idEvento, idModalidad);
            var modalidadDto = new ModalidadDto(
                modalidadCancelada.Nombre,
                modalidadCancelada.Precio,
                modalidadCancelada.Beneficios,
                modalidadCancelada.CupoMaximo
            );
            return Ok(new { mensaje = "Modalidad cancelada exitosamente.", modalidad = modalidadDto });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
