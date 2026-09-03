using Newtonsoft.Json.Converters;

namespace GestionEventos.Logica;

public class Evento
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public string Descripcion { get; private set; }
    public DateTime Fecha { get; private set; }
    public string Lugar { get; private set; }
    public bool Cancelado { get; private set; }
    private List<Modalidad> Modalidades = new List<Modalidad>();
    public Evento(string nombre, string descripcion, DateTime fecha, string lugar)
    {
        Id = Guid.NewGuid();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del evento no puede estar vacío o contener espacios en blanco");
        }

        if (fecha < DateTime.Now)
        {
            throw new ArgumentException("La fecha del evento no puede ser de un día que ya pasó");
        }

        if (string.IsNullOrWhiteSpace(lugar))
        {
            throw new ArgumentException("El lugar del evento no puede estar vacío o contener espacios en blanco");
        }

        Nombre = nombre;
        Descripcion = descripcion;
        Fecha = fecha;
        Lugar = lugar;
        Cancelado = false;
    }

    public void AgregarModalidad(Modalidad modalidad)
    {
        if (Modalidades == null)
        {
            Modalidades = new List<Modalidad>();
        }
        Modalidades.Add(modalidad);
    }

    public void EliminarrModalidad(Modalidad modalidad)
    {
        Modalidades.Remove(modalidad);
    }

    public List<Modalidad> ObtenerModalidades()
    {
        return Modalidades;
    }

    public Modalidad ObtenerModalidadPorId(Guid id)
    {
        return Modalidades.First(v => v.Id == id);
    }

    public void Cancelar()
    {
        Cancelado = true;
    }

    public bool EstaDisponible()
    {
        return !Cancelado;
    }
}