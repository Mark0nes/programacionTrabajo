namespace GestionEventos.Logica;

public class Compra
{
    public Guid Id { get; private set; }
    public string DniComprador { get; private set; }
    public DateTime FechaCompra { get; private set; }
    private List<Entrada> Entradas = new List<Entrada>();

    public Compra(string dniComprador, DateTime fechaCompra, int cantidad)
    {
        Id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(dniComprador) || (dniComprador.Length != 8))
        {
            throw new ArgumentException("El formato del dni es incorrecto");
        }

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