using System.ComponentModel.DataAnnotations;

namespace ValidacionEventos.Api.DTOs;

public class ValidarEntradaRequest
{
    [Required(ErrorMessage = "El código de la entrada es obligatorio.")]
    [StringLength(20, ErrorMessage = "El código de la entrada no puede superar los 20 caracteres.")]
    public string? Codigo { get; set; }

    /// <summary>Evento que se está controlando en la puerta. Obligatorio.</summary>
    [Required(ErrorMessage = "El evento que se está controlando es obligatorio.")]
    public Guid? IdEvento { get; set; }
}
