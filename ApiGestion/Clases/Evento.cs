using GestionEventos.Logica.Excepciones;
using Newtonsoft.Json;

namespace GestionEventos.Logica;

public class Evento
{
    // [JsonProperty] en TODAS las propiedades con "private set": sin el atributo, Newtonsoft
    // no puede asignarlas al leer eventos.json y el evento se reconstruye con datos vacíos.
    [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
    [JsonProperty] public string Nombre { get; private set; } = string.Empty;
    [JsonProperty] public string Descripcion { get; private set; } = string.Empty;
    [JsonProperty] public DateTime Fecha { get; private set; }
    [JsonProperty] public string Lugar { get; private set; } = string.Empty;
    [JsonProperty] public double? Latitud { get; private set; }
    [JsonProperty] public double? Longitud { get; private set; }
    [JsonProperty] public bool Cancelado { get; private set; }
    [JsonProperty] public List<Modalidad> Modalidades { get; private set; } = new List<Modalidad>();

    public Evento()
    {
    }

    public Evento(string nombre, string descripcion, DateTime fecha, string lugar, double? latitud = null, double? longitud = null)
    {
        ValidarDatos(nombre, lugar);

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Fecha = fecha;
        Lugar = lugar.Trim();
        Latitud = latitud;
        Longitud = longitud;
        Cancelado = false;
        Modalidades = new List<Modalidad>();
    }

    /// <summary>
    /// Las fechas que llegan del frontend vienen en UTC ("...Z"); las del archivo, sin zona.
    /// Para comparar contra la hora actual se llevan siempre a hora local.
    /// </summary>
    public static DateTime ALocal(DateTime fecha)
    {
        return fecha.Kind == DateTimeKind.Utc ? fecha.ToLocalTime() : fecha;
    }

    public void AgregarModalidad(Modalidad modalidad)
    {
        if (modalidad == null)
        {
            throw new ReglaNegocioException("La modalidad no puede ser nula.");
        }

        if (Cancelado)
        {
            throw new ReglaNegocioException("No se pueden agregar modalidades a un evento cancelado.");
        }

        Modalidades.Add(modalidad);
    }

    public Modalidad? ObtenerModalidadPorId(Guid id)
    {
        return Modalidades.FirstOrDefault(m => m.Id == id);
    }

    public void Cancelar()
    {
        Cancelado = true;
    }

    /// <summary>La fecha del evento ya pasó respecto de <paramref name="ahora"/>.</summary>
    public bool YaPaso(DateTime ahora)
    {
        return ALocal(Fecha) < ahora;
    }

    /// <summary>Se puede vender: no está cancelado y todavía no ocurrió.</summary>
    public bool EstaDisponible(DateTime ahora)
    {
        return !Cancelado && !YaPaso(ahora);
    }

    public void Actualizar(string nombre, string descripcion, DateTime fecha, string lugar, double? latitud = null, double? longitud = null)
    {
        ValidarDatos(nombre, lugar);

        Nombre = nombre.Trim();
        Descripcion = descripcion?.Trim() ?? string.Empty;
        Fecha = fecha;
        Lugar = lugar.Trim();
        Latitud = latitud;
        Longitud = longitud;
    }

    private static void ValidarDatos(string nombre, string lugar)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaNegocioException("El nombre del evento no puede estar vacío.");
        }

        if (string.IsNullOrWhiteSpace(lugar))
        {
            throw new ReglaNegocioException("El lugar del evento no puede estar vacío.");
        }
    }
}
