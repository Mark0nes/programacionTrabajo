using GestionEventos.Data;
using GestionEventos.Logica.Excepciones;

namespace GestionEventos.Logica;

public class CompraService
{
    private readonly CompraRepository _compraRepository;
    private readonly EventoRepository _eventoRepository;
    private readonly UsuarioService _usuarioService;
    private readonly Func<DateTime> _ahora;

    // "ahora" se inyecta para que los tests no dependan del reloj real.
    public CompraService(CompraRepository compraRepository, EventoRepository eventoRepository, UsuarioService usuarioService, Func<DateTime>? ahora = null)
    {
        _compraRepository = compraRepository ?? throw new ArgumentNullException(nameof(compraRepository));
        _eventoRepository = eventoRepository ?? throw new ArgumentNullException(nameof(eventoRepository));
        _usuarioService = usuarioService ?? throw new ArgumentNullException(nameof(usuarioService));
        _ahora = ahora ?? (() => DateTime.Now);
    }

    /// <summary>
    /// Detalle de una compra. El enunciado no exige rol para este endpoint, así que sin DNI se puede
    /// consultar por ID (el GUID funciona como referencia de la compra). Si se envía un DNI, se valida:
    /// un organizador puede ver cualquiera y un comprador solo las suyas.
    /// </summary>
    public Compra ObtenerPorId(Guid idCompra, string? dniSolicitante)
    {
        var compra = _compraRepository.ObtenerCompras().FirstOrDefault(c => c.Id == idCompra)
            ?? throw new EntidadNoEncontradaException($"Compra con ID '{idCompra}' no encontrada.");

        if (string.IsNullOrWhiteSpace(dniSolicitante))
        {
            return compra;
        }

        var solicitante = _usuarioService.ValidarUsuario(dniSolicitante);

        if (!solicitante.EsOrganizador() && compra.DniComprador != solicitante.Dni)
        {
            throw new AccesoDenegadoException("No tiene permiso para consultar una compra de otro comprador.");
        }

        return compra;
    }

    /// <summary>
    /// Compras visibles para quien opera: el organizador ve todas, el comprador solo las propias.
    /// </summary>
    public List<Compra> ObtenerComprasVisibles(string? dniSolicitante)
    {
        var solicitante = _usuarioService.ValidarUsuario(dniSolicitante);
        var compras = _compraRepository.ObtenerCompras();

        if (solicitante.EsOrganizador())
        {
            return compras;
        }

        return compras.Where(c => c.DniComprador == solicitante.Dni).ToList();
    }

    public Compra RealizarCompra(string? dniComprador, Guid idEvento, Guid idModalidad, int cantidad)
    {
        var comprador = _usuarioService.ValidarRol(dniComprador, RolUsuario.Comprador);

        if (cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad de entradas debe ser mayor a cero.");
        }

        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == idEvento)
            ?? throw new EntidadNoEncontradaException($"No se encontró el evento con ID '{idEvento}'.");

        DateTime ahora = _ahora();

        if (evento.Cancelado)
        {
            throw new ReglaNegocioException("No se pueden comprar entradas para un evento cancelado.");
        }

        if (evento.YaPaso(ahora))
        {
            throw new ReglaNegocioException("No se pueden comprar entradas para un evento cuya fecha ya pasó.");
        }

        var modalidad = evento.ObtenerModalidadPorId(idModalidad)
            ?? throw new EntidadNoEncontradaException($"No se encontró la modalidad con ID '{idModalidad}' en el evento.");

        // RegistrarVenta valida modalidad cancelada y cupo, y descuenta el cupo.
        modalidad.RegistrarVenta(cantidad);

        decimal total = modalidad.CalcularPrecio(cantidad);
        decimal precioPorEntrada = Math.Round(total / cantidad, 2);

        // Unicidad contra TODAS las compras existentes (y contra las que se generan en esta misma operación).
        var comprasExistentes = _compraRepository.ObtenerCompras();
        var codigosExistentes = new HashSet<string>(
            comprasExistentes.SelectMany(c => c.Entradas).Select(e => e.Codigo),
            StringComparer.OrdinalIgnoreCase);

        var nuevaCompra = new Compra(comprador.Dni, evento.Id, evento.Nombre, modalidad.Id, modalidad.Nombre, cantidad, modalidad.Precio, ahora);

        decimal acumulado = 0m;
        for (int i = 0; i < cantidad; i++)
        {
            string codigo = GeneradorCodigoEntrada.GenerarCodigoUnico(codigosExistentes);

            // La última entrada absorbe el redondeo, para que la suma de las entradas sea exactamente el total cobrado.
            decimal precio = (i == cantidad - 1) ? total - acumulado : precioPorEntrada;
            acumulado += precio;

            nuevaCompra.AgregarEntrada(new Entrada(codigo, evento.Id, evento.Nombre, modalidad.Id, modalidad.Nombre, nuevaCompra.Id, precio));
        }

        _eventoRepository.GuardarEventos(eventos);
        comprasExistentes.Add(nuevaCompra);
        _compraRepository.GuardarCompras(comprasExistentes);

        return nuevaCompra;
    }

    /// <summary>
    /// Cancela una entrada individual y devuelve su lugar al cupo. Política con descuento por volumen:
    /// la entrada cancelada se reintegra por lo que pagó (su precio efectivo) y las demás conservan el
    /// precio que ya abonaron; el total de la compra baja en ese monto.
    /// </summary>
    public Entrada CancelarEntrada(string? codigo, string? dniSolicitante)
    {
        var solicitante = _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Comprador);

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ReglaNegocioException("El código de la entrada es obligatorio.");
        }

        string codigoBuscado = codigo.Trim().ToUpperInvariant();
        var compras = _compraRepository.ObtenerCompras();

        Compra? compraContenedora = compras.FirstOrDefault(c => c.Entradas.Any(e => e.Codigo == codigoBuscado));
        Entrada? entrada = compraContenedora?.Entradas.First(e => e.Codigo == codigoBuscado);

        if (compraContenedora == null || entrada == null)
        {
            throw new EntidadNoEncontradaException($"No se encontró ninguna entrada con el código '{codigoBuscado}'.");
        }

        if (compraContenedora.DniComprador != solicitante.Dni)
        {
            throw new AccesoDenegadoException("No tiene permiso para cancelar una entrada que no le pertenece.");
        }

        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == entrada.IdEvento);

        if (!entrada.Cancelada && !entrada.Usada && evento != null && evento.YaPaso(_ahora()))
        {
            throw new ReglaNegocioException("No se puede cancelar una entrada de un evento que ya comenzó o finalizó.");
        }

        // CancelarEntrada rechaza entradas ya canceladas o ya usadas (validadas en la puerta).
        entrada.CancelarEntrada();
        compraContenedora.RecalcularTotal();

        evento?.ObtenerModalidadPorId(entrada.IdModalidad)?.RestaurarCupo(1);

        if (evento != null)
        {
            _eventoRepository.GuardarEventos(eventos);
        }

        _compraRepository.GuardarCompras(compras);

        return entrada;
    }
}
