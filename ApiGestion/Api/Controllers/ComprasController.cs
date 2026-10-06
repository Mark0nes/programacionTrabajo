using GestionEventos.Logica;
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
    public IActionResult RegistrarCompra([FromBody] RealizarCompraDto dto)
    {
        try
        {
            string? dni = ObtenerDni();
            if (string.IsNullOrWhiteSpace(dni))
            {
                dni = dto.DniComprador;
            }

            _usuarioService.ValidarRol(dni, RolUsuario.Comprador);

            var compra = _compraService.RealizarCompra(dni!, dto.IdEvento, dto.IdModalidad, dto.Cantidad);
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
        var compra = _compraService.ObtenerPorId(id);
        return compra != null
            ? Ok(compra)
            : NotFound(new { mensaje = $"Compra con ID '{id}' no encontrada." });
    }

    [HttpGet]
    public IActionResult ObtenerCompras([FromQuery] string? dni)
    {
        string? dniEfectivo = !string.IsNullOrWhiteSpace(dni) ? dni : ObtenerDni();
        if (!string.IsNullOrWhiteSpace(dniEfectivo))
        {
            return Ok(_compraService.ObtenerPorDni(dniEfectivo));
        }

        try
        {
            string? dniOrganizador = ObtenerDni();
            _usuarioService.ValidarRol(dniOrganizador, RolUsuario.Organizador);
            return Ok(_compraService.ObtenerTodas());
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Para consultar el listado completo de compras sin filtrar por DNI se requiere rol Organizador." });
        }
    }
}
