namespace GestionEventos.Api.DTOs;

public record CompraResponse(
    Guid Id,
    string DniComprador,
    DateTime FechaCompra,
    Guid IdEvento,
    string NombreEvento,
    Guid IdModalidad,
    string NombreModalidad,
    int Cantidad,
    decimal PrecioUnitario,
    decimal Total,
    List<EntradaResponse> Entradas);
