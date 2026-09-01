namespace GestionEventos.Logica;
using GestionEventos.Data;

public class EventoService
{
    private readonly EventoRepository _repository;

    public EventoService()
    {
        _repository = new EventoRepository("eventos.json");
    }

    public List<Evento> ObtenerTodos()
    {
        return _repository.ObtenerEventos();
    }

    public Evento ObtenerPorId(Guid id)
    {
        return _repository.ObtenerEventos().FirstOrDefault(e => e.Id == id);
    }

    public List<Evento> ObtenerPorFecha(DateTime fecha)
    {
        return _repository.ObtenerEventos().Where(e => e.Fecha == fecha).ToList();
    }

    public List<Evento> ObtenerPorLugar(string lugar)
    {
        return _repository.ObtenerEventos().Where(e => e.Lugar == lugar).ToList();
    }

    public List<Evento> ObtenerPorModalidad(Guid idModalidad)
    {
        return _repository.ObtenerEventos().Where(e => e.ObtenerModalidades().Any(m => m.Id == idModalidad)).ToList();
    }

    public List<Evento> ObtenerDisponibles()
    {
        return _repository.ObtenerEventos().Where(e => !e.Cancelado).ToList();
    }

    public Evento Crear(string nombre, string descripcion, DateTime fecha, string lugar)
    {
        var eventos = _repository.ObtenerEventos();
        
        var nuevoEvento = new Evento(nombre,descripcion,fecha,lugar);
        
        eventos.Add(nuevoEvento);
        _repository.GuardarEventos(eventos);
        
        return nuevoEvento;
    }
}
