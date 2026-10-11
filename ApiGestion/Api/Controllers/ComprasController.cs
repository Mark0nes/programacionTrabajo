using GestionEventos.Api.DTOs;
using GestionEventos.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class ComprasController : BaseApiController
{
    private readonly CompraService _compraService;

    public ComprasController(CompraService compraService)
    {
        _compraService = compraService;
    }

    [HttpPost]
    public IActionResult RegistrarCompra([FromBody] CompraRequest request)
    {
        return Ejecutar(() =>
        {
            var compra = _compraService.RealizarCompra(ObtenerDni(), request.IdEvento!.Value, request.IdModalidad!.Value, request.Cantidad);
            return StatusCode(StatusCodes.Status201Created, compra.AResponse());
        });
    }

    /// <summary>Detalle de la compra y estado de cada entrada. Un comprador solo ve las suyas.</summary>
    [HttpGet("{id:guid}")]
    public IActionResult ObtenerPorId(Guid id)
    {
        return Ejecutar(() => Ok(_compraService.ObtenerPorId(id, ObtenerDni()).AResponse()));
    }

    /// <summary>Compras de quien opera (X-Dni). El organizador recibe todas las compras del sistema.</summary>
    [HttpGet]
    public IActionResult ObtenerCompras()
    {
        return Ejecutar(() => Ok(_compraService.ObtenerComprasVisibles(ObtenerDni()).Select(c => c.AResponse()).ToList()));
    }
}
