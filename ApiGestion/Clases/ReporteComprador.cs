namespace GestionEventos.Logica;

public class ReporteComprador
{
    public string Dni { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public int CantidadEntradas { get; private set; }
    public decimal Total { get; private set; }

    public ReporteComprador(string dni, string nombre, int cantidadEntradas, decimal total)
    {
        Dni = dni;
        Nombre = nombre;
        CantidadEntradas = cantidadEntradas;
        Total = total;
    }

    public void AgregarEntradas(int cantidad, decimal total)
    {
        CantidadEntradas += cantidad;
        Total += total;
    }
}