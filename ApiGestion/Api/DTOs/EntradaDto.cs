namespace GestionEventos.Api.DTOs;
public record EntradaCanceladaDto(string Codigo, Guid IdEvento, Guid IdModalidad, Guid IdCompra, bool Cancelada);

public record EntradaUsadaDto(string Codigo, Guid IdEvento, Guid IdModalidad, Guid IdCompra, bool Usada, DateTime? FechaUso);