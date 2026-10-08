namespace GestionEventos.Api.DTOs;

public class ReporteModalidadDto
{
    public Guid IdModalidad { get; set; }
    public string NombreModalidad { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int CupoMaximo { get; set; }
    public int CupoDisponible { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal Recaudacion { get; set; }
    public bool Cancelada { get; set; }
}
