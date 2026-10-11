namespace GestionEventos.Api.DTOs;

public record ReporteEventoResponse(
    Guid IdEvento,
    string NombreEvento,
    DateTime Fecha,
    string Lugar,
    bool Cancelado,
    int EntradasVendidas,
    decimal RecaudacionTotal,
    List<ReporteModalidadResponse> Modalidades,
    List<ReporteCompradorResponse> Compradores);
