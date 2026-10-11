using GestionEventos.Logica;
using Newtonsoft.Json;

namespace GestionEventos.Data;

/// <summary>Repositorio de usuarios (usuarios.json, precargado).</summary>
public class PersonaRepository
{
    private readonly string _rutaArchivo;

    public PersonaRepository(string? rutaArchivo = null)
    {
        _rutaArchivo = rutaArchivo ?? DataPathHelper.ObtenerRuta("usuarios.json");
    }

    public List<Usuario> ObtenerUsuarios()
    {
        if (!File.Exists(_rutaArchivo))
        {
            return new List<Usuario>();
        }

        string json = File.ReadAllText(_rutaArchivo);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<Usuario>();
        }

        return JsonConvert.DeserializeObject<List<Usuario>>(json) ?? new List<Usuario>();
    }

    public void GuardarUsuarios(List<Usuario> usuarios)
    {
        string? directorio = Path.GetDirectoryName(_rutaArchivo);
        if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        File.WriteAllText(_rutaArchivo, JsonConvert.SerializeObject(usuarios, Formatting.Indented));
    }
}
