namespace GestionEventos.Logica;

using GestionEventos.Data;

public class ModalidadService
{
    private readonly EventoRepository _eventoRepository;

    public ModalidadService(EventoRepository? eventoRepository = null)
    {
        _eventoRepository = eventoRepository ?? new EventoRepository();
    }

    public List<Modalidad> ObtenerPorEvento(Guid idEvento)
    {
        var evento = _eventoRepository.ObtenerEventos().FirstOrDefault(e => e.Id == idEvento);
        return evento?.ObtenerModalidades() ?? new List<Modalidad>();
    }

    public Modalidad? ObtenerPorId(Guid idEvento, Guid idModalidad)
    {
        var evento = _eventoRepository.ObtenerEventos().FirstOrDefault(e => e.Id == idEvento);
        return evento?.ObtenerModalidadPorId(idModalidad);
    }
}
