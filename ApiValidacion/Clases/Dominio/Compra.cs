namespace ValidacionEventos.Dominio;

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
    public decimal Total { get; set; }

    [JsonProperty]
    [JsonInclude]
    public List<Entrada> Entradas { get; set; } = new List<Entrada>();

    public Compra()
    {
    }
}
