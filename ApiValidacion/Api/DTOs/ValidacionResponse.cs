namespace ValidacionEventos.Api.DTOs;

/// <summary>Respuesta para la puerta: si pasa o no, el motivo (Estado + Mensaje) y los datos de la entrada.</summary>
public record ValidacionResponse(
    bool Exitoso,
    string Estado,
    string Mensaje,
    string? Codigo,
    string? NombreEvento,
    string? NombreModalidad,
    DateTime? FechaUso);
