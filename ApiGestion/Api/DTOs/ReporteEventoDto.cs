namespace GestionEventos.Api.DTOs;

public record ReporteEventoDto (Guid IdEvento, string NombreEvento, DateTime Fecha, string Lugar, bool Cancelado, int EntradasVendidas, decimal RecaudacionTotal, List<ReporteModalidadDto> Modalidades, List<ReporteCompradorDto> Compradores);