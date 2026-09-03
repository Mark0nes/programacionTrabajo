namespace GestionEventos.Logica;
using GestionEventos.Data;

public class CompraService
{
    private readonly CompraRepository _repository;

    public CompraService()
    {
        _repository = new CompraRepository("compras.json");
    }

    public List<Compra> ObtenerTodos()
    {
        return _repository.ObtenerCompras();
    }

    public Compra? ObtenerPorId(Guid idCompra)
    {
        return _repository.ObtenerCompras().FirstOrDefault(c => c.Id == idCompra);
    }

    public decimal ObtenerTotalDeCompra(Guid idCompra)
    {
        return _repository.ObtenerCompras().FirstOrDefault(c => c.Id == idCompra).CalcularTotal();
    }

    public decimal ObtenerTotalRecaudadoPorEvento(Guid idEvento)
    {
        return _repository.ObtenerCompras().Where(c => c.ObtenerEventoDeCompra().Id == idEvento).CalcularTotal();
    }

    public decimal ObtenerTotalRecaudadoPorModalidad(string lugar)
    {
        return _repository.ObtenerCompras().Where(c => c.Lugar == lugar).ToList();
    }

    public decimal ObtenerTotalRecaudado(string lugar)
    {
        return _repository.ObtenerCompras().Where(e => e.Lugar == lugar).ToList();
    }

    public Evento Crear(string nombre, string descripcion, DateTime fecha, string lugar)
    {
        var eventos = _repository.ObtenerEventos();
        
        var nuevoEvento = new Evento(nombre,descripcion,fecha,lugar);
        
        eventos.Add(nuevoEvento);
        _repository.GuardarEventos(eventos);
        
        return nuevoEvento;
    }

    public Evento? AgregarModalidad(Guid idModalidad, Guid idEvento)
    {
        var eventos = _repository.ObtenerEventos();
        var eventoModificado = eventos.FirstOrDefault(e=>e.Id == idEvento);

        if (eventoModificado == null)
        {
            throw new ArgumentException("El evento a buscar no se encontró. Verifique el Id del mismo.");
        }

        eventoModificado.AgregarModalidad(eventoModificado.ObtenerModalidadPorId(idModalidad));
        return eventoModificado;
    }

    public Evento EliminarModalidad(Guid idModalidad, Guid idEvento)
    {
        var eventos = _repository.ObtenerEventos();
        var eventoModificado = eventos.FirstOrDefault(e=>e.Id == idEvento);

        if (eventoModificado == null)
        {
            throw new ArgumentException("El evento a buscar no se encontró. Verifique el Id del mismo.");
        }

        eventoModificado.EliminarrModalidad(eventoModificado.ObtenerModalidadPorId(idModalidad));
        return eventoModificado;
    }

    public Evento Cancelar(Guid idEvento)
    {
        var eventos = _repository.ObtenerEventos();
        var eventoModificado = eventos.FirstOrDefault(e=>e.Id == idEvento);

        if (eventoModificado == null)
        {
            throw new ArgumentException("El evento a buscar no se encontró. Verifique el Id del mismo.");
        }

        eventoModificado.Cancelar();
        return eventoModificado;
    }

    public bool PreguntarPorDisponibilidad(Guid idEvento)
    {
        var eventos = _repository.ObtenerEventos();
        var eventoModificado = eventos.FirstOrDefault(e=>e.Id == idEvento);

        if (eventoModificado == null)
        {
            throw new ArgumentException("El evento a buscar no se encontró. Verifique el Id del mismo.");
        }
        
        return eventoModificado.EstaDisponible();
    }
}
