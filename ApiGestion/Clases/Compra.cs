namespace GestionEventos.Logica;

public class Compra
{
    public Guid Id { get; private set; }
    public int DniComprador { get; private set; }
    public DateTime FechaCompra { get; private set; }
    private List<Entrada> Entradas = new List<Entrada>();

    public Compra(int dniComprador, DateTime fechaCompra, int cantidad)
    {
        Id = Guid.NewGuid();
        DniComprador = dniComprador;
        FechaCompra = fechaCompra;
    }

    public void AgregarEntrada(Entrada entrada)
    {
        Entradas.Add(entrada);
    }

    public List<Entrada> ObtenerEntradas()
    {
        return Entradas;
    }

    public decimal CalcularTotal()
    {
        return Entradas.Sum(e => e.CalcularPrecio());
    }
}