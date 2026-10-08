namespace GestionEventos.Logica;

public class ReporteEvento
{
    public Guid IdEvento { get; private set; }
    public string NombreEvento { get; private set; } = string.Empty;
    public DateTime Fecha { get; private set; }
    public string Lugar { get; private set; } = string.Empty;
    public bool Cancelado { get; private set; }
    public int EntradasVendidas { get; private set; }
    public decimal RecaudacionTotal { get; private set; }
    public List<ReporteModalidad> Modalidades { get; private set; } = new List<ReporteModalidad>();
    public List<ReporteComprador> Compradores { get; private set; } = new List<ReporteComprador>();

    public ReporteEvento(Guid idEvento, string nombreEvento, DateTime fecha, string lugar, bool cancelado, int entradasVendidas, decimal recaudacionTotal)
    {
        IdEvento = idEvento;
        NombreEvento = nombreEvento;
        Fecha = fecha;
        Lugar = lugar;
        Cancelado = cancelado;
        EntradasVendidas = entradasVendidas;
        RecaudacionTotal = recaudacionTotal;
    }

    public ReporteComprador ObtenerReporteComprador(string dni, string nombre, int cantidadEntradas, decimal total)
    {
        return new ReporteComprador(dni, nombre, cantidadEntradas, total);
    }
}