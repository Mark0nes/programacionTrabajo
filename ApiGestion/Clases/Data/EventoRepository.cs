namespace GestionEventos.Data;
using GestionEventos.Logica;
using Newtonsoft.Json;

public class EventoRepository
{
    private readonly string _rutaArchivo;

    public EventoRepository(string rutaArchivo)
    {
        _rutaArchivo = rutaArchivo;
    }

    public List<Evento> ObtenerEventos()
    {
        if (!File.Exists(_rutaArchivo))
        {
            return new List<Evento>();
        }

        string json = File.ReadAllText(_rutaArchivo);
        return JsonConvert.DeserializeObject<List<Evento>>(json) ?? new List<Evento>();
    }

    public void GuardarEventos(List<Evento> eventos)
    {
        string json = JsonConvert.SerializeObject(eventos, Formatting.Indented);
        File.WriteAllText(_rutaArchivo,json);
    }
}