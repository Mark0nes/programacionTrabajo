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

    public List<Modalidad> ObtenerModalidades()
    {
        return Modalidades;
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