namespace GestionEventos.Logica;

public class Entrada
{
    public string Codigo { get; set; } = string.Empty;
    public Guid IdEvento { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public Guid IdModalidad { get; set; }
    public string NombreModalidad { get; set; } = string.Empty;
    public Guid IdCompra { get; set; }
    public decimal PrecioUnitario { get; set; }
    public bool Usada { get; set; }
    public DateTime? FechaUso { get; set; }
    public bool Cancelada { get; set; }

    public Entrada()
    {
    }

    public Entrada(string codigo, Guid idEvento, string nombreEvento, Guid idModalidad, string nombreModalidad, Guid idCompra, decimal precioUnitario)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la entrada no puede estar vacío.");
        }

        Codigo = codigo.Trim().ToUpperInvariant();
        IdEvento = idEvento;
        NombreEvento = nombreEvento;
        IdModalidad = idModalidad;
        NombreModalidad = nombreModalidad;
        IdCompra = idCompra;
        PrecioUnitario = precioUnitario;
        Usada = false;
        FechaUso = null;
        Cancelada = false;
    }

    public void MarcarComoUsada()
    {
        if (Cancelada)
        {
            throw new InvalidOperationException("No se puede marcar como usada una entrada cancelada.");
        }

        if (Usada)
        {
            throw new InvalidOperationException("La entrada ya ha sido utilizada.");
        }

        Usada = true;
        FechaUso = DateTime.Now;
    }

    public void CancelarEntrada()
    {
        if (Usada)
        {
            throw new InvalidOperationException("No se puede cancelar una entrada que ya fue utilizada en la puerta.");
        }

        if (Cancelada)
        {
            throw new InvalidOperationException("La entrada ya se encuentra cancelada.");
        }

        Cancelada = true;
    }
}