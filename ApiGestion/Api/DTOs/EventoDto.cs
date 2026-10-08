namespace GestionEventos.Api.Dto;

public record CrearEventoDto(string Nombre, string Descripcion, DateTime Fecha, string Lugar, double? Latitud, double? Longitud);

public record ModificarEventoDto(string Nombre, string Descripcion, DateTime Fecha, string Lugar, double? Latitud, double? Longitud);