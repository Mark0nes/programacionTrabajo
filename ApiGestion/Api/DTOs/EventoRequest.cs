using System.ComponentModel.DataAnnotations;

namespace GestionEventos.Api.DTOs;

public class EventoRequest
{
    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(120, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 120 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "La descripción no puede superar los 1000 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La fecha del evento es obligatoria.")]
    public DateTime? Fecha { get; set; }

    [Required(ErrorMessage = "El lugar del evento es obligatorio.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "El lugar debe tener entre 2 y 200 caracteres.")]
    public string Lugar { get; set; } = string.Empty;

    [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
    public double? Latitud { get; set; }

    [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
    public double? Longitud { get; set; }
}
