namespace GestionEventos.Api.DTOs;

public record EntradaResponse(
    string Codigo,
    Guid IdEvento,
    string NombreEvento,
    Guid IdModalidad,
    string NombreModalidad,
    Guid IdCompra,
    decimal PrecioUnitario,
    bool Usada,
    DateTime? FechaUso,
    bool Cancelada);
