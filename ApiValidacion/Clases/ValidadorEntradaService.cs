namespace ValidacionEventos.Logica;

using ValidacionEventos.Data;
using ValidacionEventos.Dominio;

public class ValidadorEntradaService
{
    private readonly IValidacionCompraRepository _compraRepository;
    private readonly IValidacionEventoRepository _eventoRepository;

    public ValidadorEntradaService(IValidacionCompraRepository compraRepository, IValidacionEventoRepository eventoRepository)
    {
        _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
        _eventoRepository = eventoRepository ?? throw new ArgumentNullException(nameof(eventoRepository));
    }

    public ValidadorEntradaService(string? rutaCompras = null, string? rutaEventos = null)
        : this(new ValidacionCompraRepository(rutaCompras), new ValidacionEventoRepository(rutaEventos))
    {
    }

    public ResultadoValidacion ValidarEntrada(string codigo, Guid? idEvento = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return new ResultadoValidacion
            {
                Exitoso = false,
                Estado = EstadoValidacion.NoExiste,
                Mensaje = "El código de la entrada no puede estar vacío."
            };
        }

        string codigoBuscado = codigo.Trim().ToUpperInvariant();

        var compras = _compraRepository.ObtenerCompras();
        Entrada? entradaEncontrada = null;
        Compra? compraContenedora = null;

        foreach (var compra in compras)
        {
            var ent = compra.Entradas.FirstOrDefault(e =>
                string.Equals(e.Codigo, codigoBuscado, StringComparison.OrdinalIgnoreCase));

            if (ent != null)
            {
                entradaEncontrada = ent;
                compraContenedora = compra;
                break;
            }
        }

        if (entradaEncontrada == null)
        {
            return new ResultadoValidacion
            {
                Exitoso = false,
                Estado = EstadoValidacion.NoExiste,
                Codigo = codigoBuscado,
                Mensaje = $"La entrada con código '{codigoBuscado}' no existe en el sistema."
            };
        }

        string nombreEvento = !string.IsNullOrWhiteSpace(entradaEncontrada.NombreEvento)
            ? entradaEncontrada.NombreEvento
            : "Evento";
        string nombreModalidad = !string.IsNullOrWhiteSpace(entradaEncontrada.NombreModalidad)
            ? entradaEncontrada.NombreModalidad
            : "Modalidad";

        // 1. Entrada cancelada previamente por el comprador
        if (entradaEncontrada.Cancelada)
        {
            return new ResultadoValidacion
            {
                Exitoso = false,
                Estado = EstadoValidacion.Cancelada,
                Codigo = codigoBuscado,
                NombreEvento = nombreEvento,
                NombreModalidad = nombreModalidad,
                Mensaje = $"La entrada '{codigoBuscado}' fue cancelada previamente por el comprador."
            };
        }

        // 2. Entrada de otro evento (si se especificó idEvento)
        if (idEvento.HasValue && idEvento.Value != Guid.Empty && idEvento.Value != entradaEncontrada.IdEvento)
        {
            return new ResultadoValidacion
            {
                Exitoso = false,
                Estado = EstadoValidacion.EventoIncorrecto,
                Codigo = codigoBuscado,
                NombreEvento = nombreEvento,
                NombreModalidad = nombreModalidad,
                Mensaje = $"La entrada no corresponde a este evento (es para '{nombreEvento}')."
            };
        }

        // 3. Evento cancelado
        var evento = _eventoRepository.ObtenerPorId(entradaEncontrada.IdEvento);
        if (evento != null && evento.Cancelado)
        {
            return new ResultadoValidacion
            {
                Exitoso = false,
                Estado = EstadoValidacion.EventoCancelado,
                Codigo = codigoBuscado,
                NombreEvento = nombreEvento,
                NombreModalidad = nombreModalidad,
                Mensaje = $"El evento '{nombreEvento}' ha sido cancelado. Ingreso denegado."
            };
        }

        // 4. Entrada ya utilizada
        if (entradaEncontrada.Usada)
        {
            return new ResultadoValidacion
            {
                Exitoso = false,
                Estado = EstadoValidacion.YaFueUsada,
                Codigo = codigoBuscado,
                NombreEvento = nombreEvento,
                NombreModalidad = nombreModalidad,
                FechaUso = entradaEncontrada.FechaUso,
                Mensaje = $"La entrada ya fue utilizada el {entradaEncontrada.FechaUso:dd/MM/yyyy HH:mm:ss}."
            };
        }

        // 5. Ingreso autorizado - Marcar como usada y persistir mediante repositorio
        DateTime ahora = DateTime.Now;
        entradaEncontrada.MarcarComoUsada(ahora);
        _compraRepository.GuardarCompras(compras);

        return new ResultadoValidacion
        {
            Exitoso = true,
            Estado = EstadoValidacion.Valida,
            Codigo = codigoBuscado,
            NombreEvento = nombreEvento,
            NombreModalidad = nombreModalidad,
            FechaUso = ahora,
            Mensaje = $"Ingreso autorizado para '{nombreEvento}' — Modalidad: {nombreModalidad}."
        };
    }
}
