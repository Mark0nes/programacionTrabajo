using ValidacionEventos.Logica;

namespace ValidacionEventos.Api.DTOs;

public static class ValidacionMapper
{
    public static ValidacionResponse AResponse(this ResultadoValidacion resultado) =>
        new(resultado.Exitoso, resultado.Estado.ToString(), resultado.Mensaje, resultado.Codigo,
            resultado.NombreEvento, resultado.NombreModalidad, resultado.FechaUso);
}
