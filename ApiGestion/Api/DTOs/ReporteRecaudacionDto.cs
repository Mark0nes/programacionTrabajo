namespace GestionEventos.Api.DTOs;
public class ReporteRecaudacionDto
{
    public decimal TotalGeneralRecaudado { get; set; }
    public int TotalGeneralEntradasVendidas { get; set; }
    public List<ReporteEventoDto> Eventos { get; set; } = new List<ReporteEventoDto>();
}