namespace ValidacionEventos.Data;

using Newtonsoft.Json;
using ValidacionEventos.Dominio;

public class ValidacionCompraRepository : IValidacionCompraRepository
{
    private readonly string _rutaArchivo;
    private static readonly object Bloqueo = new();

    public ValidacionCompraRepository(string? rutaArchivo = null)
    {
        _rutaArchivo = rutaArchivo ?? DataPathHelper.ObtenerRuta("compras.json");
    }

    // Siempre se lee el archivo de nuevo (sin copia en memoria): una compra hecha hace cinco
    // minutos en la API de Gestión tiene que poder validarse ya.
    public List<Compra> ObtenerCompras()
    {
        lock (Bloqueo)
        {
            if (!File.Exists(_rutaArchivo))
            {
                return new List<Compra>();
            }

            string json = File.ReadAllText(_rutaArchivo);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Compra>();
            }

            return JsonConvert.DeserializeObject<List<Compra>>(json) ?? new List<Compra>();
        }
    }

    // Ojo: el lock solo protege entre hilos de ESTE proceso. Gestión es otro proceso y puede escribir
    // el mismo archivo a la vez (ver README, sección de concurrencia).
    public void GuardarCompras(List<Compra> compras)
    {
        lock (Bloqueo)
        {
            string? directorio = Path.GetDirectoryName(_rutaArchivo);
            if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            File.WriteAllText(_rutaArchivo, JsonConvert.SerializeObject(compras, Formatting.Indented));
        }
    }
}
