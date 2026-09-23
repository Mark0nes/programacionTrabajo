namespace GestionEventos.Logica;

public class Evento
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Lugar { get; set; } = string.Empty;
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }
    public bool Cancelado { get; set; }
    public List<Modalidad> Modalidades { get; set; } = new List<Modalidad>();

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