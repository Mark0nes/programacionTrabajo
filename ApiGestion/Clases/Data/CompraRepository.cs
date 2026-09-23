namespace GestionEventos.Data;

using GestionEventos.Logica;
using Newtonsoft.Json;

public class CompraRepository
{
    private readonly string _rutaArchivo;
    private static readonly object Bloqueo = new();

    public CompraRepository(string? rutaArchivo = null)
    {
        _rutaArchivo = rutaArchivo ?? DataPathHelper.GetFilePath("compras.json");
    }

    public List<Compra> ObtenerCompras()
    {
        lock (Bloqueo)
        {
            if (!File.Exists(_rutaArchivo))
            {
                return new List<Compra>();
            }

            string json = File.ReadAllText(_rutaArchivo);
            return JsonConvert.DeserializeObject<List<Compra>>(json) ?? new List<Compra>();
        }
    }

    public void GuardarCompras(List<Compra> compras)
    {
        lock (Bloqueo)
        {
            string? dir = Path.GetDirectoryName(_rutaArchivo);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonConvert.SerializeObject(compras, Formatting.Indented);
            File.WriteAllText(_rutaArchivo, json);
        }
    }
}