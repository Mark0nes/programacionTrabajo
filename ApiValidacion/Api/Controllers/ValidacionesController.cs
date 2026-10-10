using Microsoft.AspNetCore.Mvc;
using ValidacionEventos.Logica;
using ValidacionEventos.Api.DTOs;

namespace ValidacionEventos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ValidacionesController : ControllerBase
{
    private readonly ValidadorEntradaService _validador;

    public ValidacionesController(ValidadorEntradaService validador)
    {
        _validador = validador;
    }

    [HttpPost]
    public IActionResult Validar([FromBody] ValidarEntradaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Codigo))
        {
            return BadRequest(new ResultadoValidacion(false, EstadoValidacion.NoExiste, "El código no puede estar vacío."));
        }

        if (dto.IdEvento == Guid.Empty)
        {
            return BadRequest(new ResultadoValidacion(false, EstadoValidacion.EventoIncorrecto,
                "Debe indicar el evento que se está controlando."));
        }

        var resultado = _validador.ValidarEntrada(dto.Codigo, dto.IdEvento);

        if (resultado.Exitoso)
        {
            return Ok(resultado);
        }

        return BadRequest(resultado);
    }
}