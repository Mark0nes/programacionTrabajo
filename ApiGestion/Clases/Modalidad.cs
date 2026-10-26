using GestionEventos.Logica.Excepciones;
using Newtonsoft.Json;

namespace GestionEventos.Logica;

public class Modalidad
{
    public const int CantidadMinimaParaDescuento = 5;
    public const decimal PorcentajeDescuento = 0.15m;

    [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
    [JsonProperty] public string Nombre { get; private set; } = string.Empty;
    [JsonProperty] public decimal Precio { get; private set; }
    [JsonProperty] public string Beneficios { get; private set; } = string.Empty;
    [JsonProperty] public int CupoMaximo { get; private set; }
    [JsonProperty] public int CupoDisponible { get; private set; }
    [JsonProperty] public bool Cancelada { get; private set; }

    public Modalidad()
    {
    }

    public Modalidad(string nombre, decimal precio, string beneficios, int cupoMaximo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaNegocioException("El nombre de la modalidad no puede estar vacío.");
        }

        if (precio <= 0)
        {
            throw new ReglaNegocioException("El precio debe ser mayor a cero.");
        }

        if (cupoMaximo <= 0)
        {
            throw new ReglaNegocioException("El cupo máximo debe ser mayor a cero.");
        }

        Id = Guid.NewGuid();
        Nombre = nombre.Trim();
        Precio = precio;
        Beneficios = beneficios?.Trim() ?? string.Empty;
        CupoMaximo = cupoMaximo;
        CupoDisponible = cupoMaximo;
        Cancelada = false;
    }

    public void Cancelar()
    {
        Cancelada = true;
    }


    public void RegistrarVenta(int cantidad)
    {
        if (Cancelada)
        {
            throw new ReglaNegocioException($"La modalidad '{Nombre}' fue cancelada y no admite ventas.");
        }

        if (cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad de entradas debe ser mayor que cero.");
        }

        if (CupoDisponible < cantidad)
        {
            throw new CupoInsuficienteException($"No hay suficiente cupo disponible en '{Nombre}'. Cupo restante: {CupoDisponible}.");
        }

        CupoDisponible -= cantidad;
    }

    public void RestaurarCupo(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad a restaurar debe ser mayor que cero.");
        }

        CupoDisponible = Math.Min(CupoMaximo, CupoDisponible + cantidad);
    }

    public decimal CalcularPrecio(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ReglaNegocioException("La cantidad de entradas debe ser mayor que cero.");
        }

        decimal total = Precio * cantidad;

        if (cantidad >= CantidadMinimaParaDescuento)
        {
            total = total * (1m - PorcentajeDescuento);
        }

        return Math.Round(total, 2);
    }
}
