namespace ValidacionEventos.Dominio;

using Newtonsoft.Json;

/// <summary>
/// Vista de solo lectura de un evento (eventos.json lo escribe Gestión): a Validación solo le interesa
/// saber si fue cancelado. Nunca se vuelve a guardar el archivo de eventos desde esta API.
/// </summary>
public class Evento
{
    [JsonProperty] public Guid Id { get; private set; }
    [JsonProperty] public string Nombre { get; private set; } = string.Empty;
    [JsonProperty] public DateTime Fecha { get; private set; }
    [JsonProperty] public string Lugar { get; private set; } = string.Empty;
    [JsonProperty] public bool Cancelado { get; private set; }

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
