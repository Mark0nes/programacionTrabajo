namespace GestionEventos.Api.DTOs;

public record ReporteModalidadResponse(
    Guid IdModalidad,
    string NombreModalidad,
    decimal PrecioUnitario,
    int CupoMaximo,
    int CupoDisponible,
    int EntradasVendidas,
    decimal Recaudacion,
    bool Cancelada);
