using Newtonsoft.Json;

namespace GestionEventos.Logica;

/// <summary>
/// El anexo del enunciado trae el DNI como número en usuarios.json; en el dominio se maneja como texto.
/// </summary>
public class NewtonsoftDniConverter : JsonConverter<string>
{
    public override string ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        return reader.Value?.ToString() ?? string.Empty;
    }

    public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
    {
        writer.WriteValue(value);
    }
}
