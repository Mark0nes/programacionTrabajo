namespace ValidacionEventos.Api.DTOs;
using System.ComponentModel.DataAnnotations;

public record ValidarEntradaDto(
    [property: Required] string Codigo,
    [property: Required] Guid IdEvento);