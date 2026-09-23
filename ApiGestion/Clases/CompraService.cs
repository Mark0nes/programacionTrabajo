namespace GestionEventos.Logica;

using GestionEventos.Data;

public class CompraService
{
    private readonly CompraRepository _compraRepository;
    private readonly EventoRepository _eventoRepository;
    private readonly UsuarioService _usuarioService;

    public CompraService(CompraRepository? compraRepository = null, EventoRepository? eventoRepository = null, UsuarioService? usuarioService = null)
    {
        _compraRepository = compraRepository ?? new CompraRepository();
        _eventoRepository = eventoRepository ?? new EventoRepository();
        _usuarioService = usuarioService ?? new UsuarioService();
    }

    public List<Compra> ObtenerTodas()
    {
        return _compraRepository.ObtenerCompras();
    }

    public Compra? ObtenerPorId(Guid idCompra)
    {
        return _compraRepository.ObtenerCompras().FirstOrDefault(c => c.Id == idCompra);
    }

    public List<Compra> ObtenerPorDni(string dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return new List<Compra>();
        string dniLimpio = dni.Trim();
        return _compraRepository.ObtenerCompras()
            .Where(c => c.DniComprador.Equals(dniLimpio, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public Compra RealizarCompra(string dniComprador, Guid idEvento, Guid idModalidad, int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad de entradas debe ser mayor a cero.");
        }

        // 1. Validar que el usuario exista y sea Comprador
        var comprador = _usuarioService.ValidarRol(dniComprador, RolUsuario.Comprador);

        // 2. Buscar y validar evento
        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == idEvento);

        if (evento == null)
        {
            throw new KeyNotFoundException($"No se encontró el evento con ID '{idEvento}'.");
        }

        if (evento.Cancelado)
        {
            throw new InvalidOperationException("No se pueden comprar entradas para un evento cancelado.");
        }

        if (evento.Fecha < DateTime.Now)
        {
            throw new InvalidOperationException("No se pueden comprar entradas para un evento cuya fecha ya pasó.");
        }

        // 3. Buscar y validar modalidad
        var modalidad = evento.ObtenerModalidadPorId(idModalidad);
        if (modalidad == null)
        {
            throw new KeyNotFoundException($"No se encontró la modalidad con ID '{idModalidad}' en el evento.");
        }

        if (!modalidad.HayCupoDisponible(cantidad))
        {
            throw new InvalidOperationException($"No hay suficiente cupo disponible en '{modalidad.Nombre}'. Cupo restante: {modalidad.CupoDisponible}.");
        }

        // 4. Descontar cupo
        modalidad.RegistrarVenta(cantidad);

        // 5. Calcular total aplicando descuento por volumen (5 o más entradas: 15% de descuento)
        decimal total = modalidad.CalcularPrecio(cantidad);
        decimal precioEfectivoPorEntrada = Math.Round(total / cantidad, 2);

        // 6. Recopilar todos los códigos existentes para asegurar 100% unicidad
        var comprasExistentes = _compraRepository.ObtenerCompras();
        var codigosExistentes = new HashSet<string>(
            comprasExistentes.SelectMany(c => c.Entradas).Select(e => e.Codigo),
            StringComparer.OrdinalIgnoreCase
        );

        // 7. Crear la compra y generar cada entrada individual con su código único de 6 caracteres
        var nuevaCompra = new Compra(comprador.Dni, evento.Id, evento.Nombre, modalidad.Id, modalidad.Nombre, cantidad, modalidad.Precio, total);

        for (int i = 0; i < cantidad; i++)
        {
            string codigoUnico = GeneradorCodigoEntrada.GenerarCodigoUnico(codigosExistentes);
            var entrada = new Entrada(codigoUnico, evento.Id, evento.Nombre, modalidad.Id, modalidad.Nombre, nuevaCompra.Id, precioEfectivoPorEntrada);
            nuevaCompra.AgregarEntrada(entrada);
        }

        // 8. Persistir evento actualizado y nueva compra
        _eventoRepository.GuardarEventos(eventos);
        comprasExistentes.Add(nuevaCompra);
        _compraRepository.GuardarCompras(comprasExistentes);

        return nuevaCompra;
    }

    public Entrada CancelarEntrada(string codigo, string dniSolicitante)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la entrada es obligatorio.");
        }

        // Validar que quien solicita sea Comprador
        _usuarioService.ValidarRol(dniSolicitante, RolUsuario.Comprador);

        var compras = _compraRepository.ObtenerCompras();
        Compra? compraContenedora = null;
        Entrada? entradaEncontrada = null;

        string codigoLimpio = codigo.Trim().ToUpperInvariant();

        foreach (var compra in compras)
        {
            var ent = compra.Entradas.FirstOrDefault(e => e.Codigo.Equals(codigoLimpio, StringComparison.OrdinalIgnoreCase));
            if (ent != null)
            {
                compraContenedora = compra;
                entradaEncontrada = ent;
                break;
            }
        }

        if (entradaEncontrada == null || compraContenedora == null)
        {
            throw new KeyNotFoundException($"No se encontró ninguna entrada con el código '{codigo}'.");
        }

        // Validar que la entrada pertenezca al comprador
        if (!compraContenedora.DniComprador.Equals(dniSolicitante.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("No tiene permiso para cancelar una entrada que no le pertenece.");
        }

        if (entradaEncontrada.Cancelada)
        {
            throw new InvalidOperationException("La entrada ya se encuentra cancelada.");
        }

        if (entradaEncontrada.Usada)
        {
            throw new InvalidOperationException("No se puede cancelar una entrada que ya fue utilizada para ingresar al evento.");
        }

        // Validar que el evento no haya pasado
        var eventos = _eventoRepository.ObtenerEventos();
        var evento = eventos.FirstOrDefault(e => e.Id == entradaEncontrada.IdEvento);
        if (evento != null && evento.Fecha < DateTime.Now)
        {
            throw new InvalidOperationException("No se puede cancelar una entrada de un evento que ya finalizó o comenzó.");
        }

        // Marcar como cancelada
        entradaEncontrada.CancelarEntrada();

        // Restaurar cupo de la modalidad en el evento
        if (evento != null)
        {
            var modalidad = evento.ObtenerModalidadPorId(entradaEncontrada.IdModalidad);
            modalidad?.RestaurarCupo(1);
            _eventoRepository.GuardarEventos(eventos);
        }

        // Persistir compra actualizada
        _compraRepository.GuardarCompras(compras);

        return entradaEncontrada;
    }
}
