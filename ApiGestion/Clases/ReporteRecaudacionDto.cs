namespace GestionEventos.Logica;

public class ReporteRecaudacionDto
{
    public decimal TotalGeneralRecaudado { get; set; }
    public int TotalGeneralEntradasVendidas { get; set; }
    public List<ReporteEventoDto> Eventos { get; set; } = new List<ReporteEventoDto>();
}

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
}

public class ReporteModalidadDto
{
    public Guid IdModalidad { get; set; }
    public string NombreModalidad { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int CupoMaximo { get; set; }
    public int CupoDisponible { get; set; }
    public int EntradasVendidas { get; set; }
    public decimal Recaudacion { get; set; }
}
