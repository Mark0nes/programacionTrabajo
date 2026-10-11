using GestionEventos.Logica.Excepciones;
using Newtonsoft.Json;

namespace GestionEventos.Logica;

public class Compra
{
    [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
    [JsonProperty] public string DniComprador { get; private set; } = string.Empty;
    [JsonProperty] public DateTime FechaCompra { get; private set; }
    [JsonProperty] public Guid IdEvento { get; private set; }
    [JsonProperty] public string NombreEvento { get; private set; } = string.Empty;
    [JsonProperty] public Guid IdModalidad { get; private set; }
    [JsonProperty] public string NombreModalidad { get; private set; } = string.Empty;
    [JsonProperty] public int Cantidad { get; private set; }

    /// <summary>Precio de lista de la modalidad al momento de la compra (sin descuento).</summary>
    [JsonProperty] public decimal PrecioUnitario { get; private set; }

    /// <summary>Suma de lo pagado por las entradas que siguen vigentes (no canceladas).</summary>
    [JsonProperty] public decimal Total { get; private set; }

    [JsonProperty] public List<Entrada> Entradas { get; private set; } = new List<Entrada>();

    public Compra()
    {
    }

    public Compra(string dniComprador, Guid idEvento, string nombreEvento, Guid idModalidad, string nombreModalidad, int cantidad, decimal precioUnitario, DateTime fechaCompra)
    {
        if (string.IsNullOrWhiteSpace(dniComprador))
        {
            throw new ReglaNegocioException("El DNI del comprador es obligatorio.");
        }

        if (cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad debe ser mayor a cero.");
        }

        Id = Guid.NewGuid();
        DniComprador = dniComprador.Trim();
        FechaCompra = fechaCompra;
        IdEvento = idEvento;
        NombreEvento = nombreEvento;
        IdModalidad = idModalidad;
        NombreModalidad = nombreModalidad;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Total = 0m;
        Entradas = new List<Entrada>();
    }

    public void AgregarEntrada(Entrada entrada)
    {
        Entradas.Add(entrada);
        RecalcularTotal();
    }

    /// <summary>
    /// El total siempre es la suma de lo pagado por las entradas no canceladas. Así el dinero
    /// de la compra y la cantidad de entradas vigentes nunca quedan inconsistentes.
    /// </summary>
    public void RecalcularTotal()
    {
        Total = Entradas.Where(e => !e.Cancelada).Sum(e => e.PrecioUnitario);
    }
}
