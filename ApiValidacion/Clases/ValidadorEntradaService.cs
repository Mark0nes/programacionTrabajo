using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ValidacionEventos.Logica;

public class ValidadorEntradaService
{
    private readonly string _rutaCompras;
    private readonly string _rutaEventos;
    private static readonly object Bloqueo = new();

    public ValidadorEntradaService(string? rutaCompras = null, string? rutaEventos = null)
    {
        _rutaCompras = rutaCompras ?? DataPathHelper.GetFilePath("compras.json");
        _rutaEventos = rutaEventos ?? DataPathHelper.GetFilePath("eventos.json");
    }

    public ResultadoValidacion ValidarEntrada(string codigo, Guid? idEvento = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            return new ResultadoValidacion
            {
                Exitoso = false,
                Estado = EstadoValidacion.NoExiste,
                Mensaje = "El código de la entrada no puede estar vacío."
            };
        }

        string codigoBuscado = codigo.Trim().ToUpperInvariant();

        lock (Bloqueo)
        {
            if (!File.Exists(_rutaCompras))
            {
                return new ResultadoValidacion
                {
                    Exitoso = false,
                    Estado = EstadoValidacion.NoExiste,
                    Mensaje = $"No existe registro de compras ni entradas generadas."
                };
            }

            string jsonCompras = File.ReadAllText(_rutaCompras);
            var comprasArray = JArray.Parse(jsonCompras);

            JObject? entradaEncontrada = null;
            JObject? compraContenedora = null;

            foreach (var itemCompra in comprasArray.OfType<JObject>())
            {
                var entradasArray = itemCompra["Entradas"] as JArray;
                if (entradasArray == null) continue;

                foreach (var itemEntrada in entradasArray.OfType<JObject>())
                {
                    string? cod = itemEntrada["Codigo"]?.ToString();
                    if (string.Equals(cod, codigoBuscado, StringComparison.OrdinalIgnoreCase))
                    {
                        entradaEncontrada = itemEntrada;
                        compraContenedora = itemCompra;
                        break;
                    }
                }

                if (entradaEncontrada != null) break;
            }

            if (entradaEncontrada == null)
            {
                return new ResultadoValidacion
                {
                    Exitoso = false,
                    Estado = EstadoValidacion.NoExiste,
                    Codigo = codigoBuscado,
                    Mensaje = $"La entrada con código '{codigoBuscado}' no existe en el sistema."
                };
            }

            Guid eventoIdEntrada = Guid.Empty;
            if (Guid.TryParse(entradaEncontrada["IdEvento"]?.ToString(), out var parsedEventoId))
            {
                eventoIdEntrada = parsedEventoId;
            }

            string nombreEvento = entradaEncontrada["NombreEvento"]?.ToString() ?? "Evento";
            string nombreModalidad = entradaEncontrada["NombreModalidad"]?.ToString() ?? "Modalidad";
            bool cancelada = entradaEncontrada["Cancelada"]?.Value<bool>() ?? false;
            bool usada = entradaEncontrada["Usada"]?.Value<bool>() ?? false;
            DateTime? fechaUso = entradaEncontrada["FechaUso"]?.Value<DateTime?>();

            // 1. Si está cancelada por el comprador
            if (cancelada)
            {
                return new ResultadoValidacion
                {
                    Exitoso = false,
                    Estado = EstadoValidacion.NoExiste,
                    Codigo = codigoBuscado,
                    NombreEvento = nombreEvento,
                    NombreModalidad = nombreModalidad,
                    Mensaje = $"La entrada '{codigoBuscado}' fue cancelada previamente por el comprador."
                };
            }

            // 2. Si se especificó el ID del evento de la puerta y no coincide
            if (idEvento.HasValue && idEvento.Value != Guid.Empty && idEvento.Value != eventoIdEntrada)
            {
                return new ResultadoValidacion
                {
                    Exitoso = false,
                    Estado = EstadoValidacion.EventoIncorrecto,
                    Codigo = codigoBuscado,
                    NombreEvento = nombreEvento,
                    NombreModalidad = nombreModalidad,
                    Mensaje = $"La entrada no corresponde a este evento (es para '{nombreEvento}')."
                };
            }

            // 3. Verificar si el evento fue cancelado
            if (File.Exists(_rutaEventos))
            {
                try
                {
                    string jsonEventos = File.ReadAllText(_rutaEventos);
                    var eventosArray = JArray.Parse(jsonEventos);
                    var ev = eventosArray.OfType<JObject>().FirstOrDefault(e =>
                        string.Equals(e["Id"]?.ToString(), eventoIdEntrada.ToString(), StringComparison.OrdinalIgnoreCase));

                    if (ev != null && (ev["Cancelado"]?.Value<bool>() ?? false))
                    {
                        return new ResultadoValidacion
                        {
                            Exitoso = false,
                            Estado = EstadoValidacion.EventoCancelado,
                            Codigo = codigoBuscado,
                            NombreEvento = nombreEvento,
                            NombreModalidad = nombreModalidad,
                            Mensaje = $"El evento '{nombreEvento}' ha sido cancelado. Ingreso denegado."
                        };
                    }
                }
                catch
                {
                    // No bloquear validación si el archivo de eventos no se puede parsear
                }
            }

            // 4. Si ya fue usada
            if (usada)
            {
                return new ResultadoValidacion
                {
                    Exitoso = false,
                    Estado = EstadoValidacion.YaFueUsada,
                    Codigo = codigoBuscado,
                    NombreEvento = nombreEvento,
                    NombreModalidad = nombreModalidad,
                    FechaUso = fechaUso,
                    Mensaje = $"La entrada ya fue utilizada el {fechaUso:dd/MM/yyyy HH:mm:ss}."
                };
            }

            // 5. Entrada válida -> Marcar como usada y persistir inmediatamente
            DateTime ahora = DateTime.Now;
            entradaEncontrada["Usada"] = true;
            entradaEncontrada["FechaUso"] = ahora;

            File.WriteAllText(_rutaCompras, comprasArray.ToString(Formatting.Indented));

            return new ResultadoValidacion
            {
                Exitoso = true,
                Estado = EstadoValidacion.Valida,
                Codigo = codigoBuscado,
                NombreEvento = nombreEvento,
                NombreModalidad = nombreModalidad,
                FechaUso = ahora,
                Mensaje = $"Ingreso autorizado para '{nombreEvento}' — Modalidad: {nombreModalidad}."
            };
        }
    }
}
