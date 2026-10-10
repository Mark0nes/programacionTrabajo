namespace GestionEventos.Logica;

using Newtonsoft.Json;
using System.Text.Json.Serialization;
using GestionEventos.Logica.Excepciones;

public class Modalidad
{
    [JsonProperty]
    [JsonInclude]
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string Nombre { get; private set; } = string.Empty;

    [JsonProperty]
    [JsonInclude]
    public decimal Precio { get; private set; }

    public string Beneficios { get; private set; } = string.Empty;

    [JsonProperty]
    [JsonInclude]
    public int CupoMaximo { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public int CupoDisponible { get; private set; }

    [JsonProperty]
    [JsonInclude]
    public bool Cancelada { get; private set; } = false;

    public Modalidad()
    {
    }

    public Modalidad(string nombre, decimal precio, string beneficios, int cupoMaximo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre no puede estar vacío o contener solo espacios.");
        }

        if (precio <= 0)
        {
            throw new ArgumentException("El precio debe ser mayor a cero.");
        }

        if (cupoMaximo <= 0)
        {
            throw new ArgumentException("El cupo máximo debe ser mayor a cero.");
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

    public bool HayCupoDisponible(int cantidad = 1)
    {
        return !Cancelada && CupoDisponible >= cantidad;
    }

    public void RegistrarVenta(int cantidad)
    {
        if (Cancelada)
        {
            throw new ReglaNegocioException("No se pueden vender entradas para una modalidad cancelada.");
        }

        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad de entradas debe ser mayor que cero.");
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
            throw new ArgumentException("La cantidad a restaurar debe ser mayor que cero.");
        }

        CupoDisponible = Math.Min(CupoMaximo, CupoDisponible + cantidad);
    }

    public decimal CalcularPrecio(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad de entradas debe ser mayor que cero.");
        }

        if (cantidad >= 5)
        {
            return Math.Round(Precio * cantidad * 0.85m, 2);
        }

        return Precio * cantidad;
    }
}