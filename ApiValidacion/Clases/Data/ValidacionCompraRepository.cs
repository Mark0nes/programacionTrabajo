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

    public (Compra? Compra, Entrada? Entrada) BuscarPorCodigo(string codigo)
    {
        var compras = ObtenerCompras();
        string codigoBuscado = codigo.Trim().ToUpperInvariant();

        foreach (var compra in compras)
        {
            var entrada = compra.Entradas.FirstOrDefault(e =>
                string.Equals(e.Codigo, codigoBuscado, StringComparison.OrdinalIgnoreCase));

            if (entrada != null)
            {
                return (compra, entrada);
            }
        }

        return (null, null);
    }
}
