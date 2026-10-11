using ValidacionEventos.Data;
using ValidacionEventos.Dominio;

namespace Testing;

/// <summary>Repositorio de compras en memoria: los tests de la lógica no tocan el disco.</summary>
public class CompraRepositorioFalso : IValidacionCompraRepository
{
    public List<Compra> Compras { get; set; } = new List<Compra>();
    public int VecesGuardado { get; private set; }

    public List<Compra> ObtenerCompras() => Compras;

    public void GuardarCompras(List<Compra> compras)
    {
        Compras = compras;
        VecesGuardado++;
    }
}

/// <summary>Repositorio de eventos en memoria.</summary>
public class EventoRepositorioFalso : IValidacionEventoRepository
{
    public List<Evento> Eventos { get; set; } = new List<Evento>();

    public List<Evento> ObtenerEventos() => Eventos;

    public Evento? ObtenerPorId(Guid id) => Eventos.FirstOrDefault(e => e.Id == id);
}
