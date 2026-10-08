namespace GestionEventos.Api.DTOs;

public record CompraDto(string? DniComprador, Guid IdEvento, Guid IdModalidad, int Cantidad);