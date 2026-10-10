namespace GestionEventos.Api.DTOs;
public record ReporteRecaudacionDto (decimal TotalGeneralRecaudado, int TotalGeneralEntradasVendidas, List<ReporteEventoDto> Eventos);

public record ReporteEventoDto (Guid IdEvento, string NombreEvento, DateTime Fecha, string Lugar, bool Cancelado, int EntradasVendidas, decimal RecaudacionTotal, List<ReporteModalidadDto> Modalidades, List<ReporteCompradorDto> Compradores);
public record ReporteModalidadDto (Guid IdModalidad, string NombreModalidad, decimal PrecioUnitario, int CupoMaximo, int CupoDisponible, int EntradasVendidas, decimal Recaudacion, bool Cancelada);
public record ReporteCompradorDto (string Dni, string Nombre, int CantidadEntradas, decimal Total);