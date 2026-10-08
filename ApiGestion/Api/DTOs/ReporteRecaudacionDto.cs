namespace GestionEventos.Api.DTOs;
public record ReporteRecaudacionDto (decimal TotalGeneralRecaudado, int TotalGeneralEntradasVendidas, List<ReporteEventoDto> Eventos);