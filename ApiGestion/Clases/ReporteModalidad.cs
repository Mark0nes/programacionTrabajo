namespace GestionEventos.Logica;

public class ReporteModalidad
{
    public Guid IdModalidad { get; private set; }
    public string NombreModalidad { get; private set; }
    public decimal PrecioUnitario { get; private set; }
    public int CupoMaximo { get; private set; }
    public int CupoDisponible { get; private set; }
    public int EntradasVendidas { get; private set; }
    public decimal Recaudacion { get; private set; }
    public bool Cancelada { get; private set; }

    public ReporteModalidad(Guid idModalidad, string nombreModalidad, decimal precioUnitario, int cupoMaximo, int cupoDisponible, int entradasVendidas, decimal recaudacion, bool cancelada)
    {
        IdModalidad = idModalidad;
        NombreModalidad = nombreModalidad;
        PrecioUnitario = precioUnitario;
        CupoMaximo = cupoMaximo;
        CupoDisponible = cupoDisponible;
        EntradasVendidas = entradasVendidas;
        Recaudacion = recaudacion;
        Cancelada = cancelada;
    }
}
