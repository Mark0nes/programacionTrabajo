namespace GestionEventos.Api.DTOs;

/// <summary>Cuerpo de toda respuesta de error: el motivo viaja siempre en "error".</summary>
public record ErrorResponse(string Error);
