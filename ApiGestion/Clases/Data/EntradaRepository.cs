namespace GestionEventos.Data;
using GestionEventos.Logica;
using Newtonsoft.Json;

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

        string json = File.ReadAllText(_rutaArchivo);
        return JsonConvert.DeserializeObject<List<Entrada>>(json) ?? new List<Entrada>();
    }

    public void GuardarEntradas(List<Entrada> entradas)
    {
        string json = JsonConvert.SerializeObject(entradas, Formatting.Indented);
        File.WriteAllText(_rutaArchivo,json);
    }
}