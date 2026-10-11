namespace GestionEventos.Api.DTOs;

public record ModalidadResponse(Guid Id, string Nombre, decimal Precio, string Beneficios, int CupoMaximo, int CupoDisponible, bool Cancelada);
