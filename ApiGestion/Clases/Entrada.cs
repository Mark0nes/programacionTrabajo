namespace GestionEventos.Logica;

public class Entrada
{
    public string Codigo { get; private set; }
    public Guid IdEvento { get; private set; }
    public Modalidad Modalidad { get; private set; }
    public Guid IdCompra { get; private set; }
    public bool Usada { get; private set; }
    public DateTime FechaUso { get; private set; }

    public Entrada(string codigo, Guid idEvento, Modalidad modalidad, Guid idCompra)
    {
        Codigo = codigo;
        IdEvento = idEvento;
        Modalidad = modalidad;
        IdCompra = idCompra;
        Usada = false;
    }

    public void MarcarComoUsada()
    {
        Usada = true;
        FechaUso = DateTime.Now;
    }

    public bool ValidarIngreso(Guid idEvento)
    {
        if (IdEvento != idEvento)
        {
            throw new Exception("La entrada no es válida para este evento.");
        }
        if (Usada)
        {
            return false;
        }
        return true;
    }

    public decimal CalcularPrecio()
    {
        return Modalidad.Precio;
    }
}