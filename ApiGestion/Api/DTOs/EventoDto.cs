namespace GestionEventos.Api.DTOs;

public record EventoDto(string Nombre, string Descripcion, DateTime Fecha, string Lugar, double? Latitud, double? Longitud);