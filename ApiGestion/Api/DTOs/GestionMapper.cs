using GestionEventos.Logica;

namespace GestionEventos.Api.DTOs;

/// <summary>Convierte entidades del dominio en respuestas HTTP. Vive en Api: la lógica no conoce los DTOs.</summary>
public static class GestionMapper
{
    public static UsuarioResponse AResponse(this Usuario usuario) =>
        new(usuario.Dni, usuario.Nombre, usuario.Username, usuario.Rol);

    public static ModalidadResponse AResponse(this Modalidad modalidad) =>
        new(modalidad.Id, modalidad.Nombre, modalidad.Precio, modalidad.Beneficios, modalidad.CupoMaximo, modalidad.CupoDisponible, modalidad.Cancelada);

    public static EventoResponse AResponse(this Evento evento) =>
        new(evento.Id, evento.Nombre, evento.Descripcion, evento.Fecha, evento.Lugar, evento.Latitud, evento.Longitud, evento.Cancelado,
            evento.Modalidades.Select(m => m.AResponse()).ToList());

    public static EntradaResponse AResponse(this Entrada entrada) =>
        new(entrada.Codigo, entrada.IdEvento, entrada.NombreEvento, entrada.IdModalidad, entrada.NombreModalidad, entrada.IdCompra,
            entrada.PrecioUnitario, entrada.Usada, entrada.FechaUso, entrada.Cancelada);

    public static CompraResponse AResponse(this Compra compra) =>
        new(compra.Id, compra.DniComprador, compra.FechaCompra, compra.IdEvento, compra.NombreEvento, compra.IdModalidad, compra.NombreModalidad,
            compra.Cantidad, compra.PrecioUnitario, compra.Total,
            compra.Entradas.Select(e => e.AResponse()).ToList());

    public static ReporteRecaudacionResponse AResponse(this ReporteRecaudacion reporte) =>
        new(reporte.TotalGeneralRecaudado, reporte.TotalGeneralEntradasVendidas,
            reporte.Eventos.Select(e => e.AResponse()).ToList());

    public static ReporteEventoResponse AResponse(this ReporteEvento evento) =>
        new(evento.IdEvento, evento.NombreEvento, evento.Fecha, evento.Lugar, evento.Cancelado, evento.EntradasVendidas, evento.RecaudacionTotal,
            evento.Modalidades.Select(m => m.AResponse()).ToList(),
            evento.Compradores.Select(c => c.AResponse()).ToList());

    public static ReporteModalidadResponse AResponse(this ReporteModalidad modalidad) =>
        new(modalidad.IdModalidad, modalidad.NombreModalidad, modalidad.PrecioUnitario, modalidad.CupoMaximo, modalidad.CupoDisponible,
            modalidad.EntradasVendidas, modalidad.Recaudacion, modalidad.Cancelada);

    public static ReporteCompradorResponse AResponse(this ReporteComprador comprador) =>
        new(comprador.Dni, comprador.Nombre, comprador.CantidadEntradas, comprador.Total);
}
