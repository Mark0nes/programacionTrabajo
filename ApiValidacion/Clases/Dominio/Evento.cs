namespace ValidacionEventos.Dominio;

using Newtonsoft.Json;
using System.Text.Json.Serialization;

public class Evento
{
    [JsonProperty]
    [JsonInclude]
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Lugar { get; set; } = string.Empty;

    [JsonProperty]
    [JsonInclude]
    public bool Cancelado { get; private set; }

    public Evento()
    {
    }

    public Evento(Guid id, string nombre, DateTime fecha, string lugar, bool cancelado = false)
    {
        Id = id;
        Nombre = nombre;
        Fecha = fecha;
        Lugar = lugar;
        Cancelado = cancelado;
    }
}
