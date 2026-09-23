namespace GestionEventos.Logica;

public class Compra
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DniComprador { get; set; } = string.Empty;
    public DateTime FechaCompra { get; set; } = DateTime.Now;
    public Guid IdEvento { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public Guid IdModalidad { get; set; }
    public string NombreModalidad { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public List<Entrada> Entradas { get; set; } = new List<Entrada>();

    public Compra()
    {
    }

    public Compra(string dniComprador, Guid idEvento, string nombreEvento, Guid idModalidad, string nombreModalidad, int cantidad, decimal precioUnitario, decimal total)
    {
        if (string.IsNullOrWhiteSpace(dniComprador))
        {
            throw new ArgumentException("El DNI del comprador es obligatorio.");
        }

        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad debe ser mayor a cero.");
        }

        Id = Guid.NewGuid();
        DniComprador = dniComprador.Trim();
        FechaCompra = DateTime.Now;
        IdEvento = idEvento;
        NombreEvento = nombreEvento;
        IdModalidad = idModalidad;
        NombreModalidad = nombreModalidad;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Total = total;
        Entradas = new List<Entrada>();
    }

    public void AgregarEntrada(Entrada entrada)
    {
        Entradas ??= new List<Entrada>();
        Entradas.Add(entrada);
    }
}