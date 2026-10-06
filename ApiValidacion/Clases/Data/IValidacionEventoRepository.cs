namespace ValidacionEventos.Data;

using ValidacionEventos.Dominio;

public interface IValidacionEventoRepository
{
    List<Evento> ObtenerEventos();
    Evento? ObtenerPorId(Guid id);
}
