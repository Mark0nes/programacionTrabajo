namespace GestionEventos.Data;

using GestionEventos.Logica;
using Newtonsoft.Json;

public class EventoRepository
{
    private readonly string _rutaArchivo;
    private static readonly object Bloqueo = new();

    public EventoRepository(string? rutaArchivo = null)
    {
        _rutaArchivo = rutaArchivo ?? DataPathHelper.GetFilePath("eventos.json");
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
            return JsonConvert.DeserializeObject<List<Evento>>(json) ?? new List<Evento>();
        }
    }

    public void GuardarEventos(List<Evento> eventos)
    {
        lock (Bloqueo)
        {
            string? dir = Path.GetDirectoryName(_rutaArchivo);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string json = JsonConvert.SerializeObject(eventos, Formatting.Indented);
            File.WriteAllText(_rutaArchivo, json);
        }
    }
}