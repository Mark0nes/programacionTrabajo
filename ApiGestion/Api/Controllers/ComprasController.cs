using GestionEventos.Logica;
using GestionEventos.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class ComprasController : BaseApiController
{
    private readonly CompraService _compraService;
    private readonly UsuarioService _usuarioService;

    public ComprasController(CompraService compraService, UsuarioService usuarioService)
    {
        _compraService = compraService;
        _usuarioService = usuarioService;
    }

    [HttpPost]
    public IActionResult RegistrarCompra([FromBody] CompraDto dto)
    {
        try
        {
            string? dni = ObtenerDni();
            _usuarioService.ValidarRol(dni, RolUsuario.Comprador);

            var compra = _compraService.RealizarCompra(dni!, dto.IdEvento, dto.IdModalidad, dto.Cantidad);
            var compraDto = new CompraDto(
                compra.DniComprador,
                compra.IdEvento,
                compra.IdModalidad,
                compra.Cantidad
            );
            return Created($"/api/compras/{compra.Id}", compra);
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

    [HttpGet("{id:guid}")]
    public IActionResult ObtenerPorId(Guid id)
    {
        var usuario = _usuarioService.ObtenerPorDni(ObtenerDni());
        if (usuario == null)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { error = "Debe indicar un DNI válido para consultar una compra." });
        }

        var compra = _compraService.ObtenerPorId(id);
        if (compra == null)
        {
            return NotFound(new { mensaje = $"Compra con ID '{id}' no encontrada." });
        }

        if (usuario.Rol != RolUsuario.Organizador &&
            !compra.DniComprador.Equals(usuario.Dni, StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { error = "No tiene permiso para consultar una compra de otro comprador." });
        }

        var compraDto = new CompraDto(
                compra.DniComprador,
                compra.IdEvento,
                compra.IdModalidad,
                compra.Cantidad
            );

        return Ok(compraDto);
    }

    [HttpGet]
    public IActionResult ObtenerCompras([FromQuery] string? dni)
    {
        var usuario = _usuarioService.ObtenerPorDni(ObtenerDni());
        if (usuario == null)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { error = "Debe indicar un DNI válido para consultar compras." });
        }

        string? dniSolicitado = string.IsNullOrWhiteSpace(dni) ? null : dni.Trim();

        if (usuario.Rol != RolUsuario.Organizador &&
            dniSolicitado != null &&
            !dniSolicitado.Equals(usuario.Dni, StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { error = "Un comprador solo puede consultar sus propias compras." });
        }

        if (usuario.Rol == RolUsuario.Organizador && dniSolicitado == null)
        {
            return Ok(_compraService.ObtenerTodas());
        }

        return Ok(_compraService.ObtenerPorDni(dniSolicitado ?? usuario.Dni));
    }
}
