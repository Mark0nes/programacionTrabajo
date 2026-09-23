namespace GestionEventos.Data;

using GestionEventos.Logica;
using Newtonsoft.Json;

public class PersonaRepository
{
    private readonly string _rutaArchivo;

    public PersonaRepository(string? rutaArchivo = null)
    {
        _rutaArchivo = rutaArchivo ?? DataPathHelper.GetFilePath("usuarios.json");
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

    public void GuardarUsuarios(List<Usuario> usuarios)
    {
        string? dir = Path.GetDirectoryName(_rutaArchivo);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string json = JsonConvert.SerializeObject(usuarios, Formatting.Indented);
        File.WriteAllText(_rutaArchivo, json);
    }
}