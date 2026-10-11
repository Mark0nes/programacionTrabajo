using System.ComponentModel.DataAnnotations;

namespace GestionEventos.Api.DTOs;

/// <summary>
/// El comprador NO viaja en el body: se identifica con el header X-Dni, igual que en el resto de los endpoints.
/// </summary>
public class CompraRequest
{
    [Required(ErrorMessage = "El evento es obligatorio.")]
    public Guid? IdEvento { get; set; }

    [Required(ErrorMessage = "La modalidad es obligatoria.")]
    public Guid? IdModalidad { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    public int Cantidad { get; set; }
}
