namespace GestionEventos.Api.DTOs;

public record ReporteModalidadDto (Guid IdModalidad, string NombreModalidad, decimal PrecioUnitario, int CupoMaximo, int CupoDisponible, int EntradasVendidas, decimal Recaudacion, bool Cancelada);
