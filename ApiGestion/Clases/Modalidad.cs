namespace GestionEventos.Logica;

public class Modalidad
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; }
    public decimal Precio { get; private set; }
    public string Beneficios { get; private set; }
    public int CupoMaximo { get; private set; }
    public int CupoDisponible { get; private set; }

    public Modalidad(string nombre, decimal precio, string beneficios, int cupoMaximo)
    {
        Id = Guid.NewGuid();
        Nombre = nombre;
        Precio = precio;
        Beneficios = beneficios;
        CupoMaximo = cupoMaximo;
        CupoDisponible = cupoMaximo;
    }

    public bool HayCupoDisponible()
    {
        return CupoDisponible > 0;
    }

    public void RegistrarVenta(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad de entradas deben ser mayores que cero.");
        }
        if (CupoDisponible - cantidad < 0)
        {
            throw new InvalidOperationException("No hay suficiente cupo disponible.");
        }

        CupoDisponible -= cantidad;
    }

    public void CancelarVenta(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad de entradas deben ser mayores que cero.");
        }
        if (CupoDisponible + cantidad > CupoMaximo)
        {
            throw new InvalidOperationException("No se puede cancelar la venta, excede el cupo máximo.");
        }

        CupoDisponible += cantidad;
    }

    public decimal CalcularPrecio(int cantidad)
    {
        if (cantidad <= 0)
        {
            throw new ArgumentException("La cantidad de entradas deben ser mayores que cero.");
        }
        if (cantidad > 4)
        {
            return Precio * cantidad * 0.85m;
        }
        return Precio * cantidad;
    }
}