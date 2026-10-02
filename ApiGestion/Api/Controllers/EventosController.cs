using GestionEventos.Logica;
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
        return Ok(_eventoService.ObtenerTodos());
    }

    [HttpGet("{id:guid}")]
    public IActionResult ObtenerPorId(Guid id)
    {
        var evento = _eventoService.ObtenerPorId(id);
        return evento != null
            ? Ok(evento)
            : NotFound(new { mensaje = $"Evento con ID '{id}' no encontrado." });
    }

    [HttpPost]
    public IActionResult Crear([FromBody] CrearEventoDto dto)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var nuevoEvento = _eventoService.Crear(dto.Nombre, dto.Descripcion, dto.Fecha, dto.Lugar, dto.Latitud, dto.Longitud);
            return Created($"/api/eventos/{nuevoEvento.Id}", nuevoEvento);
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
    public IActionResult Modificar(Guid id, [FromBody] ModificarEventoDto dto)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var eventoModificado = _eventoService.Modificar(id, dto.Nombre, dto.Descripcion, dto.Fecha, dto.Lugar, dto.Latitud, dto.Longitud);
            return Ok(eventoModificado);
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
            return Ok(new { mensaje = "Evento cancelado exitosamente.", evento = eventoCancelado });
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
    public IActionResult AgregarModalidad(Guid id, [FromBody] CrearModalidadDto dto)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Organizador);

            var modalidad = _eventoService.AgregarModalidad(id, dto.Nombre, dto.Precio, dto.Beneficios, dto.CupoMaximo);
            return Created($"/api/eventos/{id}/modalidades/{modalidad.Id}", modalidad);
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
            return Ok(new { mensaje = "Modalidad cancelada exitosamente.", modalidad = modalidadCancelada });
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
