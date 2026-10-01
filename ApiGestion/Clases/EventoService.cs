namespace GestionEventos.Logica;

using GestionEventos.Data;

public class EventoService
{
    private readonly EventoRepository _eventoRepository;
    private readonly CompraRepository _compraRepository;
    private readonly UsuarioService _usuarioService;

    public EventoService(EventoRepository? eventoRepository = null, CompraRepository? compraRepository = null, UsuarioService? usuarioService = null)
    {
        _eventoRepository = eventoRepository ?? new EventoRepository();
        _compraRepository = compraRepository ?? new CompraRepository();
        _usuarioService = usuarioService ?? new UsuarioService();
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
            throw new KeyNotFoundException($"No se encontró ningún evento con ID '{id}'.");
        }

        if (evento.Cancelado)
        {
            throw new InvalidOperationException("No se puede modificar un evento que ya fue cancelado.");
        }

        if (fecha < DateTime.Now)
        {
            throw new ArgumentException("La fecha del evento no puede ser anterior a la fecha y hora actual.");
        }

        evento.Nombre = nombre.Trim();
        evento.Descripcion = descripcion?.Trim() ?? string.Empty;
        evento.Fecha = fecha;
        evento.Lugar = lugar.Trim();
        evento.Latitud = latitud;
        evento.Longitud = longitud;

        _eventoRepository.GuardarEventos(eventos);
        return evento;
    }

    public Evento Cancelar(Guid id)
    {
        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == id);

        if (evento == null)
        {
            throw new KeyNotFoundException($"No se encontró ningún evento con ID '{id}'.");
        }

        if (evento.Cancelado)
        {
            return evento; // Ya estaba cancelado
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
            throw new KeyNotFoundException($"No se encontró ningún evento con ID '{idEvento}'.");
        }

        if (evento.Cancelado)
        {
            throw new InvalidOperationException("No se pueden agregar modalidades a un evento cancelado.");
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
            throw new KeyNotFoundException($"No se encontró ningún evento con ID '{idEvento}'.");
        }

        var modalidad = evento.ObtenerModalidadPorId(idModalidad);
        if (modalidad == null)
        {
            throw new KeyNotFoundException($"No se encontró ninguna modalidad con ID '{idModalidad}' en el evento.");
        }

        modalidad.Cancelar();
        _eventoRepository.GuardarEventos(eventos);
        return modalidad;
    }

    public ReporteRecaudacionDto ObtenerReporteRecaudacion()
    {
        var eventos = _eventoRepository.ObtenerEventos();
        var compras = _compraRepository.ObtenerCompras();
        var usuarios = _usuarioService.ObtenerTodos();

        var reporte = new ReporteRecaudacionDto();

        foreach (var evento in eventos)
        {
            var comprasEvento = compras.Where(c => c.IdEvento == evento.Id).ToList();

            var repEvento = new ReporteEventoDto
            {
                IdEvento = evento.Id,
                NombreEvento = evento.Nombre,
                Fecha = evento.Fecha,
                Lugar = evento.Lugar,
                Cancelado = evento.Cancelado,
                EntradasVendidas = comprasEvento.Sum(c => c.Entradas.Count(e => !e.Cancelada)),
                RecaudacionTotal = comprasEvento.Sum(c => c.Total)
            };

            // Agrupar compradores que adquirieron entradas para este evento
            foreach (var compra in comprasEvento)
            {
                int cantActivas = compra.Entradas.Count(e => !e.Cancelada);
                if (cantActivas > 0)
                {
                    var nombreUsuario = usuarios.FirstOrDefault(u => u.Dni.Equals(compra.DniComprador, StringComparison.OrdinalIgnoreCase))?.Nombre ?? $"DNI {compra.DniComprador}";
                    var existente = repEvento.Compradores.FirstOrDefault(c => c.Dni.Equals(compra.DniComprador, StringComparison.OrdinalIgnoreCase));
                    if (existente != null)
                    {
                        existente.CantidadEntradas += cantActivas;
                        existente.Total += compra.Total;
                    }
                    else
                    {
                        repEvento.Compradores.Add(new ReporteCompradorDto
                        {
                            Dni = compra.DniComprador,
                            Nombre = nombreUsuario,
                            CantidadEntradas = cantActivas,
                            Total = compra.Total
                        });
                    }
                }
            }

            foreach (var mod in evento.Modalidades)
            {
                var comprasMod = comprasEvento.Where(c => c.IdModalidad == mod.Id).ToList();
                int vendidas = comprasMod.Sum(c => c.Entradas.Count(e => !e.Cancelada));
                decimal recaudado = comprasMod.Sum(c => c.Total);

                repEvento.Modalidades.Add(new ReporteModalidadDto
                {
                    IdModalidad = mod.Id,
                    NombreModalidad = mod.Nombre,
                    PrecioUnitario = mod.Precio,
                    CupoMaximo = mod.CupoMaximo,
                    CupoDisponible = mod.CupoDisponible,
                    EntradasVendidas = vendidas,
                    Recaudacion = recaudado,
                    Cancelada = mod.Cancelada
                });
            }

            reporte.Eventos.Add(repEvento);
        }

        reporte.TotalGeneralRecaudado = reporte.Eventos.Sum(e => e.RecaudacionTotal);
        reporte.TotalGeneralEntradasVendidas = reporte.Eventos.Sum(e => e.EntradasVendidas);

        return reporte;
    }
}
