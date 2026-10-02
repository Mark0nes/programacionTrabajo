using Microsoft.AspNetCore.Mvc;
using ValidacionEventos.Logica;

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
            return BadRequest(new ResultadoValidacion
            {
                Exitoso = false,
                Estado = EstadoValidacion.NoExiste,
                Mensaje = "El código de entrada es obligatorio."
            });
        }

        var resultado = _validador.ValidarEntrada(dto.Codigo, dto.IdEvento);

        if (resultado.Exitoso)
        {
            return Ok(resultado);
        }

        return BadRequest(resultado);
    }
}

public record ValidarEntradaDto(string Codigo, Guid? IdEvento);
