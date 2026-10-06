namespace ValidacionEventos.Dominio;

using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class Entrada
{
    [JsonProperty]
    [JsonInclude]
    public string Codigo { get; private set; } = string.Empty;

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
    public Guid IdCompra { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public decimal PrecioUnitario { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public bool Usada { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public DateTime? FechaUso { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public bool Cancelada { get; private set; }

    public Entrada()
    {
    }

    public Entrada(string codigo, Guid idEvento, string nombreEvento, Guid idModalidad, string nombreModalidad, Guid idCompra, decimal precioUnitario)
    {
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

    public void MarcarComoUsada(DateTime? fecha = null)
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
        FechaUso = fecha ?? DateTime.Now;
    }
}
