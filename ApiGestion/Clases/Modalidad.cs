namespace GestionEventos.Logica;

public class Modalidad
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Beneficios { get; set; } = string.Empty;
    public int CupoMaximo { get; set; }
    public int CupoDisponible { get; set; }

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
    }

    public bool HayCupoDisponible(int cantidad = 1)
    {
        return CupoDisponible >= cantidad;
    }

    public void RegistrarVenta(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad de entradas debe ser mayor que cero.");
        }

        if (CupoDisponible < cantidad)
        {
            throw new InvalidOperationException("No hay suficiente cupo disponible.");
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

        // Si alguien compra 5 entradas o más de la misma modalidad en una sola compra,
        // el precio total tiene un descuento del 15%
        if (cantidad >= 5)
        {
            return Math.Round(Precio * cantidad * 0.85m, 2);
        }

        return Precio * cantidad;
    }
}