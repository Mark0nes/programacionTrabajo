namespace GestionEventos.Logica;

public class Entrada
{
    public string Codigo { get; private set; }
    public Evento EventoEntrada { get; private set; }
    public Modalidad? ModalidadEntrada { get; private set; }
    public Guid IdCompra { get; private set; }
    public bool Usada { get; private set; }
    public DateTime FechaUso { get; private set; }

    public Entrada(string codigo, Evento evento, Modalidad modalidadEntrada, Guid idCompra)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la entrada no puede estar vacío o contener espacios en blanco");
        }

        if (modalidadEntrada == null)
        {
            throw new ArgumentException("La modalidad no puede ser nula");
        }

        Codigo = codigo;
        EventoEntrada = evento;
        ModalidadEntrada = modalidadEntrada;
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
        if (EventoEntrada.Id != idEvento)
        {
            throw new ArgumentException("La entrada no es válida para este evento.");
        }
        if (Usada)
        {
            return false;
        }
        return true;
    }

    public decimal CalcularPrecio()
    {
        return ModalidadEntrada.Precio;
    }
}