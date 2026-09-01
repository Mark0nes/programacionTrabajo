namespace GestionEventos.Data;
using GestionEventos.Logica;
using Newtonsoft.Json;

public class PersonaRepository
{
    private readonly string _rutaArchivo;

    public PersonaRepository(string rutaArchivo)
    {
        _rutaArchivo = rutaArchivo;
    }

    public List<Usuario> ObtenerUsuarios()
    {
        if (!File.Exists(_rutaArchivo))
        {
            return new List<Usuario>();
        }

        string json = File.ReadAllText(_rutaArchivo);
        return JsonConvert.DeserializeObject<List<Usuario>>(json) ?? new List<Usuario>();
    }

    public void GuardarPersonas(List<Usuario> usuarios)
    {
        string json = JsonConvert.SerializeObject(usuarios, Formatting.Indented);
        File.WriteAllText(_rutaArchivo,json);
    }
}