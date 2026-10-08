namespace GestionEventos.Api.DTOs;

public class ReporteEventoDto
{
    public Guid IdEvento { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public bool Cancelado { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal RecaudacionTotal { get; set; }
    public List<ReporteModalidadDto> Modalidades { get; set; } = new List<ReporteModalidadDto>();
    public List<ReporteCompradorDto> Compradores { get; set; } = new List<ReporteCompradorDto>();
}