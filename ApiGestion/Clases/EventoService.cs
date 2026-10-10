namespace GestionEventos.Logica;

using GestionEventos.Data;
using GestionEventos.Logica.Excepciones;

public class EventoService
{
    private readonly EventoRepository _eventoRepository;
    private readonly CompraRepository _compraRepository;
    private readonly UsuarioService _usuarioService;

    public EventoService(EventoRepository eventoRepository, CompraRepository compraRepository, UsuarioService usuarioService)
    {
        _eventoRepository = eventoRepository ?? throw new ArgumentNullException(nameof(eventoRepository));
        _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
    }

    public List<Evento> ObtenerTodos()
    {
        return _eventoRepository.ObtenerEventos();
    }

    public Evento? ObtenerPorId(Guid id)
    {
        return _eventoRepository.ObtenerEventos().FirstOrDefault(e => e.Id == id);
    }

    public List<Evento> ObtenerDisponibles()
    {
        return _eventoRepository.ObtenerEventos().Where(e => e.EstaDisponible()).ToList();
    }

    public Evento Crear(string nombre, string descripcion, DateTime fecha, string lugar, double? latitud = null, double? longitud = null)
    {
        if (fecha < DateTime.Now)
        {
            throw new ArgumentException("La fecha del evento no puede ser anterior a la fecha y hora actual.");
        }

        var nuevoEvento = new Evento(nombre, descripcion, fecha, lugar, latitud, longitud);
        var eventos = _eventoRepository.ObtenerEventos();
        eventos.Add(nuevoEvento);
        _eventoRepository.GuardarEventos(eventos);

        return nuevoEvento;
    }

    public Evento Modificar(Guid id, string nombre, string descripcion, DateTime fecha, string lugar, double? latitud = null, double? longitud = null)
    {
        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == id);

        if (evento == null)
        {
            throw new EntidadNoEncontradaException($"No se encontró ningún evento con ID '{id}'.");
        }

        if (evento.Cancelado)
        {
            throw new ReglaNegocioException("No se puede modificar un evento que ya fue cancelado.");
        }

        if (fecha < DateTime.Now)
        {
            throw new ArgumentException("La fecha del evento no puede ser anterior a la fecha y hora actual.");
        }

        evento.Actualizar(nombre, descripcion, fecha, lugar, latitud, longitud);

        _eventoRepository.GuardarEventos(eventos);
        return evento;
    }

    public Evento Cancelar(Guid id)
    {
        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == id);

        if (evento == null)
        {
            throw new EntidadNoEncontradaException($"No se encontró ningún evento con ID '{id}'.");
        }

        if (evento.Cancelado)
        {
            return evento;
        }

        evento.Cancelar();
        _eventoRepository.GuardarEventos(eventos);
        return evento;
    }

    public Modalidad AgregarModalidad(Guid idEvento, string nombre, decimal precio, string beneficios, int cupoMaximo)
    {
        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == idEvento);

        if (evento == null)
        {
            throw new EntidadNoEncontradaException($"No se encontró ningún evento con ID '{idEvento}'.");
        }

        if (evento.Cancelado)
        {
            throw new ReglaNegocioException("No se pueden agregar modalidades a un evento cancelado.");
        }

        var modalidad = new Modalidad(nombre, precio, beneficios, cupoMaximo);
        evento.AgregarModalidad(modalidad);

        _eventoRepository.GuardarEventos(eventos);
        return modalidad;
    }

    public Modalidad CancelarModalidad(Guid idEvento, Guid idModalidad)
    {
        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == idEvento);

        if (evento == null)
        {
            throw new EntidadNoEncontradaException($"No se encontró ningún evento con ID '{idEvento}'.");
        }

        var modalidad = evento.ObtenerModalidadPorId(idModalidad);
        if (modalidad == null)
        {
            throw new EntidadNoEncontradaException($"No se encontró ninguna modalidad con ID '{idModalidad}' en el evento.");
        }

        modalidad.Cancelar();
        _eventoRepository.GuardarEventos(eventos);
        return modalidad;
    }

    public ReporteRecaudacion ObtenerReporteRecaudacion()
    {
        var eventos = _eventoRepository.ObtenerEventos();
        var compras = _compraRepository.ObtenerCompras();
        var usuarios = _usuarioService.ObtenerTodos();

        var reporte = new ReporteRecaudacion();

        foreach (var evento in eventos)
        {
            var comprasEvento = compras.Where(c => c.IdEvento == evento.Id).ToList();

            var repEvento = new ReporteEvento(evento.Id, evento.Nombre, evento.Fecha, evento.Lugar, evento.Cancelado, 0, 0);

            foreach (var compra in comprasEvento)
            {
                var entradasActivas = compra.Entradas.Where(e => !e.Cancelada).ToList();
                int cantActivas = entradasActivas.Count;
                if (cantActivas > 0)
                {
                    decimal totalActivo = entradasActivas.Sum(e => e.PrecioUnitario);
                    var nombreUsuario = usuarios.FirstOrDefault(u => u.Dni.Equals(compra.DniComprador, StringComparison.OrdinalIgnoreCase))?.Nombre ?? $"DNI {compra.DniComprador}";
                    var existente = repEvento.Compradores.FirstOrDefault(c => c.Dni.Equals(compra.DniComprador, StringComparison.OrdinalIgnoreCase));
                    if (existente != null)
                    {
                        existente.AgregarEntradas(cantActivas, totalActivo);
                    }
                    else
                    {
                        repEvento.Compradores.Add(new ReporteComprador(compra.DniComprador, nombreUsuario, cantActivas, totalActivo));
                    }
                }
            }

            foreach (var mod in evento.Modalidades)
            {
                var comprasMod = comprasEvento.Where(c => c.IdModalidad == mod.Id).ToList();
                int vendidas = comprasMod.SelectMany(c => c.Entradas).Count(e => !e.Cancelada);
                decimal recaudado = comprasMod.SelectMany(c => c.Entradas).Where(e => !e.Cancelada).Sum(e => e.PrecioUnitario);

                repEvento.Modalidades.Add(new ReporteModalidad(mod.Id, mod.Nombre, mod.Precio, mod.CupoMaximo, mod.CupoDisponible, vendidas, recaudado, mod.Cancelada));
            }

            reporte.Eventos.Add(repEvento);
        }

        reporte.ActualizarTotales();

        return reporte;
    }
}
