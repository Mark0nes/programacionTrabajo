using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace GestionEventos.Logica;

public class DniJsonConverter : System.Text.Json.Serialization.JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            return reader.GetInt64().ToString();
        }
        return reader.GetString() ?? string.Empty;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public class NewtonsoftDniConverter : Newtonsoft.Json.JsonConverter<string>
{
    public override string ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        return reader.Value?.ToString() ?? string.Empty;
    }

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, string? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}

public class Usuario
{
    [System.Text.Json.Serialization.JsonConverter(typeof(DniJsonConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(NewtonsoftDniConverter))]
    [JsonProperty("dni")]
    [JsonPropertyName("dni")]
    public string Dni { get; set; } = string.Empty;

    [JsonProperty("nombre")]
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [JsonProperty("username")]
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
    [JsonProperty("rol")]
    [JsonPropertyName("rol")]
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

        Dni = dni.Trim();
        Nombre = nombre.Trim();
        Username = username.Trim();
        Rol = rol;
    }

    public bool EsOrganizador() => Rol == RolUsuario.Organizador;
    public bool EsComprador() => Rol == RolUsuario.Comprador;
}
