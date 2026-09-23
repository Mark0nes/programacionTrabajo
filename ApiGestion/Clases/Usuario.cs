using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GestionEventos.Logica;

public class Usuario
{
    private string _dni = string.Empty;

    [JsonProperty("dni")]
    public object DniRaw
    {
        get => _dni;
        set => _dni = value?.ToString()?.Trim() ?? string.Empty;
    }

    [JsonIgnore]
    public string Dni
    {
        get => _dni;
        set => _dni = value?.Trim() ?? string.Empty;
    }

    public string Nombre { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    public RolUsuario Rol { get; set; }

    public Usuario()
    {
    }

    public Usuario(string dni, string nombre, string username, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(dni))
        {
            throw new ArgumentException("El DNI no puede estar vacío.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre no puede estar vacío.");
        }

        Dni = dni;
        Nombre = nombre;
        Username = username;
        Rol = rol;
    }

    public bool EsOrganizador()
    {
        return Rol == RolUsuario.Organizador;
    }

    public bool EsComprador()
    {
        return Rol == RolUsuario.Comprador;
    }
}
