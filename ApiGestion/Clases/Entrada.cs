using GestionEventos.Logica.Excepciones;
using Newtonsoft.Json;

namespace GestionEventos.Logica;

public class Entrada
{
    [JsonProperty] public string Codigo { get; private set; } = string.Empty;
    [JsonProperty] public Guid IdEvento { get; private set; }
    [JsonProperty] public string NombreEvento { get; private set; } = string.Empty;
    [JsonProperty] public Guid IdModalidad { get; private set; }
    [JsonProperty] public string NombreModalidad { get; private set; } = string.Empty;
    [JsonProperty] public Guid IdCompra { get; private set; }

    /// <summary>Precio efectivamente pagado por esta entrada (ya con el descuento por volumen).</summary>
    [JsonProperty] public decimal PrecioUnitario { get; private set; }

    [JsonProperty] public bool Usada { get; private set; }
    [JsonProperty] public DateTime? FechaUso { get; private set; }
    [JsonProperty] public bool Cancelada { get; private set; }

    public Entrada()
    {
    }

    public Entrada(string codigo, Guid idEvento, string nombreEvento, Guid idModalidad, string nombreModalidad, Guid idCompra, decimal precioUnitario)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ReglaNegocioException("El código de la entrada no puede estar vacío.");
        }

        Codigo = codigo.Trim().ToUpperInvariant();
        IdEvento = idEvento;
        NombreEvento = nombreEvento;
        IdModalidad = idModalidad;
        NombreModalidad = nombreModalidad;
        IdCompra = idCompra;
        PrecioUnitario = precioUnitario;
        Usada = false;
        FechaUso = null;
        Cancelada = false;
    }

    public void MarcarComoUsada(DateTime fecha)
    {
        if (Cancelada)
        {
            throw new ReglaNegocioException("No se puede marcar como usada una entrada cancelada.");
        }

        if (Usada)
        {
            throw new ReglaNegocioException("La entrada ya ha sido utilizada.");
        }

        Usada = true;
        FechaUso = fecha;
    }

    public void CancelarEntrada()
    {
        if (Usada)
        {
            throw new ReglaNegocioException("No se puede cancelar una entrada que ya fue utilizada para ingresar al evento.");
        }

        if (Cancelada)
        {
            throw new ReglaNegocioException("La entrada ya se encuentra cancelada.");
        }

        Cancelada = true;
    }
}
