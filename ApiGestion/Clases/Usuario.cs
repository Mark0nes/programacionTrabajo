using GestionEventos.Logica.Excepciones;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GestionEventos.Logica;

public class Usuario
{
    [JsonConverter(typeof(NewtonsoftDniConverter))]
    [JsonProperty("dni")]
    public string Dni { get; private set; } = string.Empty;

    [JsonProperty("nombre")]
    public string Nombre { get; private set; } = string.Empty;

    [JsonProperty("username")]
    public string Username { get; private set; } = string.Empty;

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty("rol")]
    public RolUsuario Rol { get; private set; }

    public Usuario()
    {
    }

    public Usuario(string dni, string nombre, string username, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(dni))
        {
            throw new ReglaNegocioException("El DNI no puede estar vacío.");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ReglaNegocioException("El nombre no puede estar vacío.");
        }

        Dni = dni.Trim();
        Nombre = nombre.Trim();
        Username = username?.Trim() ?? string.Empty;
        Rol = rol;
    }

    public bool EsOrganizador() => Rol == RolUsuario.Organizador;

    public bool EsComprador() => Rol == RolUsuario.Comprador;
}
