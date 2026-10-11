using GestionEventos.Api.DTOs;
using GestionEventos.Logica;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class EntradasController : BaseApiController
{
    private readonly CompraService _compraService;

    public EntradasController(CompraService compraService)
    {
        _compraService = compraService;
    }

    /// <summary>(Promoción) Cancela una entrada propia no usada y devuelve el cupo a la modalidad.</summary>
    [HttpDelete("{codigo}")]
    public IActionResult CancelarEntrada(string codigo)
    {
        return Ejecutar(() =>
        {
            var entrada = _compraService.CancelarEntrada(codigo, ObtenerDni());
            return Ok(new AccionResponse<EntradaResponse>("Entrada cancelada con éxito. El cupo ha sido restablecido.", entrada.AResponse()));
        });
    }
}
