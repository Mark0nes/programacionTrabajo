namespace GestionEventos.Api.DTOs;

public class ReporteCompradorDto
{
    public string Dni { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public int CantidadEntradas { get; set; }
    public decimal Total { get; set; }
}