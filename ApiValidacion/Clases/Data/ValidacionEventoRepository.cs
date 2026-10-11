namespace ValidacionEventos.Data;

using Newtonsoft.Json;
using ValidacionEventos.Dominio;

public class ValidacionEventoRepository : IValidacionEventoRepository
{
    private readonly string _rutaArchivo;
    private static readonly object Bloqueo = new();

    public ValidacionEventoRepository(string? rutaArchivo = null)
    {
        _rutaArchivo = rutaArchivo ?? DataPathHelper.ObtenerRuta("eventos.json");
    }

    public List<Evento> ObtenerEventos()
    {
        lock (Bloqueo)
        {
            if (!File.Exists(_rutaArchivo))
            {
                return new List<Evento>();
            }

            string json = File.ReadAllText(_rutaArchivo);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Evento>();
            }

            return JsonConvert.DeserializeObject<List<Evento>>(json) ?? new List<Evento>();
        }
    }

    public Evento? ObtenerPorId(Guid id)
    {
        return ObtenerEventos().FirstOrDefault(e => e.Id == id);
    }
}
