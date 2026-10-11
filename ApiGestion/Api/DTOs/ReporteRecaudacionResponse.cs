namespace GestionEventos.Api.DTOs;

public record ReporteRecaudacionResponse(decimal TotalGeneralRecaudado, int TotalGeneralEntradasVendidas, List<ReporteEventoResponse> Eventos);
