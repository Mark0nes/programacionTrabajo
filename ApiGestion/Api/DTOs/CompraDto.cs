namespace GestionEventos.Api.Dto;

public record RealizarCompraDto(string? DniComprador, Guid IdEvento, Guid IdModalidad, int Cantidad);