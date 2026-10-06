namespace GestionEventos.Logica;

using Newtonsoft.Json;
using System.Text.Json.Serialization;
using GestionEventos.Logica.Excepciones;

public class Evento
{
    [JsonProperty]
    [JsonInclude]
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }

    [JsonProperty]
    [JsonInclude]
    public bool Cancelado { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public List<Modalidad> Modalidades { get; private set; } = new List<Modalidad>();

    public Evento()
    {
    }

    public Evento(string nombre, string descripcion, DateTime fecha, string lugar, double? latitud = null, double? longitud = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del evento no puede estar vacío.");
        }

        if (string.IsNullOrWhiteSpace(lugar))
        {
            throw new ArgumentException("El lugar del evento no puede estar vacío.");
        }

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

    public void AgregarModalidad(Modalidad modalidad)
    {
        if (modalidad == null)
        {
            throw new ArgumentNullException(nameof(modalidad), "La modalidad no puede ser nula.");
        }

        if (Cancelado)
        {
            throw new ReglaNegocioException("No se pueden agregar modalidades a un evento cancelado.");
        }

        Modalidades ??= new List<Modalidad>();
        Modalidades.Add(modalidad);
    }

    public void EliminarModalidad(Guid idModalidad)
    {
        Modalidades?.RemoveAll(m => m.Id == idModalidad);
    }

    public List<Modalidad> ObtenerModalidades()
    {
        return Modalidades ?? new List<Modalidad>();
    }

    public Modalidad? ObtenerModalidadPorId(Guid id)
    {
        return Modalidades?.FirstOrDefault(m => m.Id == id);
    }

    public void Cancelar()
    {
        Cancelado = true;
    }

    public bool EstaDisponible()
    {
        return !Cancelado && Fecha >= DateTime.Now;
    }
}