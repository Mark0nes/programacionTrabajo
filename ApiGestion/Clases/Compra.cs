namespace GestionEventos.Logica;

using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class Compra
{
    [JsonProperty]
    [JsonInclude]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [JsonProperty]
    [JsonInclude]
    public string DniComprador { get; private set; } = string.Empty;

    [JsonProperty]
    [JsonInclude]
    public DateTime FechaCompra { get; private set; } = DateTime.Now;

    [JsonProperty]
    [JsonInclude]
    public Guid IdEvento { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public string NombreEvento { get; private set; } = string.Empty;

    [JsonProperty]
    [JsonInclude]
    public Guid IdModalidad { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public string NombreModalidad { get; private set; } = string.Empty;

    [JsonProperty]
    [JsonInclude]
    public int Cantidad { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public decimal PrecioUnitario { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public decimal Total { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public List<Entrada> Entradas { get; private set; } = new List<Entrada>();

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

    public void RecalcularTotal()
    {
        Total = Entradas?.Where(e => !e.Cancelada).Sum(e => e.PrecioUnitario) ?? 0m;
    }
}