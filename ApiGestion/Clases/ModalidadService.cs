namespace GestionEventos.Logica;
using GestionEventos.Data;

public class ModalidadService
{
    private readonly ModalidadRepository _repository;

    public ModalidadService()
    {
        _repository = new ModalidadRepository("modalidades.json");
    }

    public List<Modalidad> ObtenerTodos()
    {
        return _repository.ObtenerModa();
    }

    public Evento ObtenerPorId(Guid id)
    {
        return _repository.ObtenerEventos().FirstOrDefault(e => e.Id == id);
    }

    public List<Modalidad> ObtenerPorFecha(DateTime fecha)
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

    public Evento AgregarModalidad(Modalidad modalidad, Guid idEvento)
    {
        var eventos = _repository.ObtenerEventos();
        var eventoModificado = eventos.FirstOrDefault(e=>e.Id == idEvento);
        eventoModificado.AgregarModalidad(modalidad);
        return eventoModificado;
    }

    public Evento Cancelar(Guid idEvento)
    {
        var eventos = _repository.ObtenerEventos();
        var eventoModificado = eventos.FirstOrDefault(e=>e.Id == idEvento);
        eventoModificado.Cancelar();
        return eventoModificado;
    }

    public bool PreguntarPorDisponibilidad(Guid idEvento)
    {
        var eventos = _repository.ObtenerEventos();
        var eventoModificado = eventos.FirstOrDefault(e=>e.Id == idEvento);
        return eventoModificado.EstaDisponible();
    }
}
