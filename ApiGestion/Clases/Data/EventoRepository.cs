using GestionEventos.Logica;
using Newtonsoft.Json;

namespace GestionEventos.Data;

public class EventoRepository
{
    private readonly string _rutaArchivo;
    private static readonly object Bloqueo = new();

    public EventoRepository(string? rutaArchivo = null)
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

    public void GuardarEventos(List<Evento> eventos)
    {
        lock (Bloqueo)
        {
            string? directorio = Path.GetDirectoryName(_rutaArchivo);
            if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
            {
                Directory.CreateDirectory(directorio);
            }

            File.WriteAllText(_rutaArchivo, JsonConvert.SerializeObject(eventos, Formatting.Indented));
        }
    }
}
