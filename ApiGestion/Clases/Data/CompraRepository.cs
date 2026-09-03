namespace GestionEventos.Data;
using GestionEventos.Logica;
using Newtonsoft.Json;

public class CompraRepository
{
    private readonly string _rutaArchivo;

    public CompraRepository(string rutaArchivo)
    {
        _rutaArchivo = rutaArchivo;
    }

    public List<Compra> ObtenerCompras()
    {
        if (!File.Exists(_rutaArchivo))
        {
            return new List<Compra>();
        }

        string json = File.ReadAllText(_rutaArchivo);
        return JsonConvert.DeserializeObject<List<Compra>>(json) ?? new List<Compra>();
    }

    public void GuardarCompras(List<Compra> compras)
    {
        string json = JsonConvert.SerializeObject(compras, Formatting.Indented);
        File.WriteAllText(_rutaArchivo,json);
    }
}