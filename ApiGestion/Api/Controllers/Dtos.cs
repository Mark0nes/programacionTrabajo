namespace GestionEventos.Api.Controllers;

public record CrearEventoDto(string Nombre, string Descripcion, DateTime Fecha, string Lugar, double? Latitud, double? Longitud);
public record ModificarEventoDto(string Nombre, string Descripcion, DateTime Fecha, string Lugar, double? Latitud, double? Longitud);
public record CrearModalidadDto(string Nombre, decimal Precio, string Beneficios, int CupoMaximo);
public record RealizarCompraDto(string? DniComprador, Guid IdEvento, Guid IdModalidad, int Cantidad);
