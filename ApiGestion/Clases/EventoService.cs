using GestionEventos.Data;
using GestionEventos.Logica.Excepciones;

namespace GestionEventos.Logica;

public class EventoService
{
    private readonly EventoRepository _eventoRepository;
    private readonly CompraRepository _compraRepository;
    private readonly UsuarioService _usuarioService;
    private readonly Func<DateTime> _ahora;

    public EventoService(EventoRepository eventoRepository, CompraRepository compraRepository, UsuarioService usuarioService, Func<DateTime>? ahora = null)
    {
        _eventoRepository = eventoRepository ?? throw new ArgumentNullException(nameof(eventoRepository));
        _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _ahora = ahora ?? (() => DateTime.Now);
    }

    /// <summary>
    /// Eventos vigentes (no cancelados) para el control de acceso en puerta.
    /// Permite seleccionar y validar entradas de eventos que ya comenzaron o se desarrollan hoy,
    /// a diferencia del catálogo de compras que solo lista eventos futuros.
    /// </summary>
    public List<Evento> ObtenerParaControlAcceso()
    {
        return _eventoRepository.ObtenerEventos()
            .Where(e => !e.Cancelado)
            .OrderByDescending(e => e.Fecha)
            .ToList();
    }

    public List<Evento> ObtenerParaGestion(string? dniSolicitante)
    {
        _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Organizador);
        return _eventoRepository.ObtenerEventos();
    }

    public List<Evento> ObtenerDisponibles()
    {
        DateTime ahora = _ahora();
        return _eventoRepository.ObtenerEventos().Where(e => e.EstaDisponible(ahora)).ToList();
    }

    public Evento ObtenerPorId(Guid id)
    {
        return _eventoRepository.ObtenerEventos().FirstOrDefault(e => e.Id == id)
            ?? throw new EntidadNoEncontradaException($"No se encontró ningún evento con ID '{id}'.");
    }

    public Evento Crear(string? dniSolicitante, string nombre, string descripcion, DateTime fecha, string lugar, double? latitud = null, double? longitud = null)
    {
        _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Organizador);
        ValidarFechaFutura(fecha);

        var nuevoEvento = new Evento(nombre, descripcion, fecha, lugar, latitud, longitud);
        var eventos = _eventoRepository.ObtenerEventos();
        eventos.Add(nuevoEvento);
        _eventoRepository.GuardarEventos(eventos);

        return nuevoEvento;
    }

    public Evento Modificar(string? dniSolicitante, Guid id, string nombre, string descripcion, DateTime fecha, string lugar, double? latitud = null, double? longitud = null)
    {
        _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Organizador);

        var eventos = _eventoRepository.ObtenerEventos();
        var evento = BuscarEvento(eventos, id);

        if (evento.Cancelado)
        {
            throw new ReglaNegocioException("No se puede modificar un evento que ya fue cancelado.");
        }

        ValidarFechaFutura(fecha);

        evento.Actualizar(nombre, descripcion, fecha, lugar, latitud, longitud);
        _eventoRepository.GuardarEventos(eventos);
        return evento;
    }

    public Evento Cancelar(string? dniSolicitante, Guid id)
    {
        _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Organizador);

        var eventos = _eventoRepository.ObtenerEventos();
        var evento = BuscarEvento(eventos, id);

        if (evento.Cancelado)
        {
            return evento;
        }

        evento.Cancelar();
        _eventoRepository.GuardarEventos(eventos);
        return evento;
    }

    public Modalidad AgregarModalidad(string? dniSolicitante, Guid idEvento, string nombre, decimal precio, string beneficios, int cupoMaximo)
    {
        _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Organizador);

        var eventos = _eventoRepository.ObtenerEventos();
        var evento = BuscarEvento(eventos, idEvento);

        var modalidad = new Modalidad(nombre, precio, beneficios, cupoMaximo);
        evento.AgregarModalidad(modalidad);

        _eventoRepository.GuardarEventos(eventos);
        return modalidad;
    }

    public Modalidad CancelarModalidad(string? dniSolicitante, Guid idEvento, Guid idModalidad)
    {
        _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Organizador);

        var eventos = _eventoRepository.ObtenerEventos();
        var evento = BuscarEvento(eventos, idEvento);

        var modalidad = evento.ObtenerModalidadPorId(idModalidad)
            ?? throw new EntidadNoEncontradaException($"No se encontró ninguna modalidad con ID '{idModalidad}' en el evento.");

        modalidad.Cancelar();
        _eventoRepository.GuardarEventos(eventos);
        return modalidad;
    }

    public ReporteRecaudacion ObtenerReporteRecaudacion(string? dniSolicitante)
    {
        _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Organizador);

        var eventos = _eventoRepository.ObtenerEventos();
        var compras = _compraRepository.ObtenerCompras();
        var usuarios = _usuarioService.ObtenerTodos();

        var reporte = new ReporteRecaudacion();

        foreach (var evento in eventos)
        {
            var comprasEvento = compras.Where(c => c.IdEvento == evento.Id).ToList();
            var repEvento = new ReporteEvento(evento.Id, evento.Nombre, evento.Fecha, evento.Lugar, evento.Cancelado);

            foreach (var compra in comprasEvento)
            {
                var entradasVigentes = compra.Entradas.Where(e => !e.Cancelada).ToList();
                if (entradasVigentes.Count == 0)
                {
                    continue;
                }

                string nombreComprador = usuarios.FirstOrDefault(u => u.Dni == compra.DniComprador)?.Nombre
                                         ?? $"DNI {compra.DniComprador}";
                repEvento.RegistrarEntradas(compra.DniComprador, nombreComprador, entradasVigentes.Count, entradasVigentes.Sum(e => e.PrecioUnitario));
            }

            foreach (var modalidad in evento.Modalidades)
            {
                var entradasModalidad = comprasEvento
                    .Where(c => c.IdModalidad == modalidad.Id)
                    .SelectMany(c => c.Entradas)
                    .Where(e => !e.Cancelada)
                    .ToList();

                repEvento.Modalidades.Add(new ReporteModalidad(
                    modalidad.Id, modalidad.Nombre, modalidad.Precio, modalidad.CupoMaximo, modalidad.CupoDisponible,
                    entradasModalidad.Count, entradasModalidad.Sum(e => e.PrecioUnitario), modalidad.Cancelada));
            }

            reporte.AgregarEvento(repEvento);
        }

        return reporte;
    }

    private void ValidarFechaFutura(DateTime fecha)
    {
        if (Evento.ALocal(fecha) < _ahora())
        {
            throw new ReglaNegocioException("La fecha del evento no puede ser anterior a la fecha y hora actual.");
        }
    }

    private static Evento BuscarEvento(List<Evento> eventos, Guid id)
    {
        return eventos.FirstOrDefault(e => e.Id == id)
            ?? throw new EntidadNoEncontradaException($"No se encontró ningún evento con ID '{id}'.");
    }
}
