namespace GestionEventos.Data;
using GestionEventos.Logica;

public class EntradaRepository
{
    private readonly string _rutaArchivo;

    public EntradaRepository(string rutaArchivo)
    {
        _rutaArchivo = rutaArchivo;
    }

    public List<Entrada> ObtenerEntradas()
    {
        if (!File.Exists(_rutaArchivo))
        {
            return new List<Entrada>();
        }
    }
}