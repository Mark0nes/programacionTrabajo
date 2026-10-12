using System.ComponentModel.DataAnnotations;

namespace GestionEventos.Api.DTOs;

public class ModalidadRequest
{
    [Required(ErrorMessage = "El nombre de la modalidad es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(0.01, 1000000000.0, ErrorMessage = "El precio debe ser mayor a cero.")]
    public decimal Precio { get; set; }

    [StringLength(500, ErrorMessage = "Los beneficios no pueden superar los 500 caracteres.")]
    public string? Beneficios { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El cupo máximo debe ser mayor a cero.")]
    public int CupoMaximo { get; set; }
}
