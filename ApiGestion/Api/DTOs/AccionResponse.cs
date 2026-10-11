namespace GestionEventos.Api.DTOs;

/// <summary>Respuesta de una acción (cancelar, etc.): mensaje para mostrar + el recurso resultante.</summary>
public record AccionResponse<T>(string Mensaje, T Resultado);
