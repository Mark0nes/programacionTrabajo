namespace GestionEventos.Api.DTOs;

public record EventoResponse(
    Guid Id,
    string Nombre,
    string Descripcion,
    DateTime Fecha,
    string Lugar,
    double? Latitud,
    double? Longitud,
    bool Cancelado,
    List<ModalidadResponse> Modalidades);
