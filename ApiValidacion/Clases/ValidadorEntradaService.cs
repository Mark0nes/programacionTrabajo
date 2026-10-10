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

    public ResultadoValidacion ValidarEntrada(string codigo, Guid idEvento)
    {
        if (idEvento == Guid.Empty)
        {
            return new ResultadoValidacion(false, EstadoValidacion.EventoIncorrecto, "Debe especificarse un evento válido para realizar la validación.");
        }
        
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return new ResultadoValidacion(false, EstadoValidacion.NoExiste, "El código no puede estar vacío.");
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
            return new ResultadoValidacion(false, EstadoValidacion.NoExiste, $"La entrada con código '{codigoBuscado}' no existe en el sistema.", codigoBuscado);
        }

        string nombreEvento = !string.IsNullOrWhiteSpace(entradaEncontrada.NombreEvento)
            ? entradaEncontrada.NombreEvento
            : "Evento";
        string nombreModalidad = !string.IsNullOrWhiteSpace(entradaEncontrada.NombreModalidad)
            ? entradaEncontrada.NombreModalidad
            : "Modalidad";

        if (entradaEncontrada.Cancelada)
        {
            return new ResultadoValidacion(false, EstadoValidacion.Cancelada, $"La entrada '{codigoBuscado}' fue cancelada previamente por el comprador.", codigoBuscado, nombreEvento, nombreModalidad);
        }

        if (idEvento != entradaEncontrada.IdEvento)
        {
            return new ResultadoValidacion(false, EstadoValidacion.EventoIncorrecto, $"La entrada no corresponde a este evento (es para '{nombreEvento}').", codigoBuscado, nombreEvento, nombreModalidad);
        }

        var evento = _eventoRepository.ObtenerPorId(entradaEncontrada.IdEvento);
        if (evento != null && evento.Cancelado)
        {
            return new ResultadoValidacion(false, EstadoValidacion.EventoCancelado, $"El evento '{nombreEvento}' ha sido cancelado. Ingreso denegado.", codigoBuscado, nombreEvento, nombreModalidad);
        }

        if (entradaEncontrada.Usada)
        {
            return new ResultadoValidacion(false, EstadoValidacion.YaFueUsada, $"La entrada ya fue utilizada el {entradaEncontrada.FechaUso:dd/MM/yyyy HH:mm:ss}.", codigoBuscado, nombreEvento, nombreModalidad, entradaEncontrada.FechaUso);
        }

        DateTime ahora = DateTime.Now;
        entradaEncontrada.MarcarComoUsada(ahora);
        _compraRepository.GuardarCompras(compras);

        return new ResultadoValidacion(true, EstadoValidacion.Valida, $"Ingreso autorizado para '{nombreEvento}' — Modalidad: {nombreModalidad}.", codigoBuscado, nombreEvento, nombreModalidad, ahora);
    }
}
