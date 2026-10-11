namespace GestionEventos.Logica;

public class ReporteEvento
{
    public Guid IdEvento { get; private set; }
    public string NombreEvento { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Lugar { get; private set; }
    public bool Cancelado { get; private set; }
    public List<ReporteModalidad> Modalidades { get; private set; } = new List<ReporteModalidad>();
    public List<ReporteComprador> Compradores { get; private set; } = new List<ReporteComprador>();

    /// <summary>Entradas vigentes (no canceladas) vendidas para el evento.</summary>
    public int EntradasVendidas => Compradores.Sum(c => c.CantidadEntradas);

    /// <summary>Dinero de las entradas vigentes. Se calcula con la misma base que la cantidad, así nunca difieren.</summary>
    public decimal RecaudacionTotal => Compradores.Sum(c => c.Total);

    public ReporteEvento(Guid idEvento, string nombreEvento, DateTime fecha, string lugar, bool cancelado)
    {
        IdEvento = idEvento;
        NombreEvento = nombreEvento;
        Fecha = fecha;
        Lugar = lugar;
        Cancelado = cancelado;
    }

    public void RegistrarEntradas(string dni, string nombre, int cantidad, decimal total)
    {
        var existente = Compradores.FirstOrDefault(c => c.Dni == dni);
        if (existente != null)
        {
            existente.AgregarEntradas(cantidad, total);
        }
        else
        {
            Compradores.Add(new ReporteComprador(dni, nombre, cantidad, total));
        }
    }
}
