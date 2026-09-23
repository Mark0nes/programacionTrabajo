using GestionEventos.Data;
using GestionEventos.Logica;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "API Gestión de Eventos y Ventas",
        Version = "v1",
        Description = "API para la gestión de eventos, venta de entradas individuales con descuento por volumen y reportes de recaudación."
    });

    // Permite ingresar el DNI en Swagger para probar endpoints con autorización de rol
    c.AddSecurityDefinition("DniHeader", new OpenApiSecurityScheme
    {
        Name = "X-Dni",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Ingrese el DNI del usuario para validar su rol (ej: 30111222 para Organizador, 40123456 para Comprador)"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "DniHeader"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Habilitar CORS para permitir llamadas del frontend
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Registrar servicios de lógica
builder.Services.AddSingleton<PersonaRepository>();
builder.Services.AddSingleton<EventoRepository>();
builder.Services.AddSingleton<CompraRepository>();
builder.Services.AddSingleton<EntradaRepository>();
builder.Services.AddSingleton<UsuarioService>();
builder.Services.AddSingleton<EventoService>();
builder.Services.AddSingleton<CompraService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

// Helper para extraer DNI desde Header X-Dni o Query param 'dni'
string? ObtenerDni(HttpContext context)
{
    if (context.Request.Headers.TryGetValue("X-Dni", out var headerDni) && !string.IsNullOrWhiteSpace(headerDni))
    {
        return headerDni.ToString();
    }
    if (context.Request.Query.TryGetValue("dni", out var queryDni) && !string.IsNullOrWhiteSpace(queryDni))
    {
        return queryDni.ToString();
    }
    return null;
}

// -------------------------------------------------------------
// 1. Usuarios
// -------------------------------------------------------------
app.MapGet("/api/usuarios", (UsuarioService usuarioService) =>
{
    return Results.Ok(usuarioService.ObtenerTodos());
})
.WithName("ObtenerUsuarios")
.WithTags("Usuarios")
.WithSummary("Lista todos los usuarios precargados.");

// -------------------------------------------------------------
// 2. Eventos
// -------------------------------------------------------------
app.MapGet("/api/eventos", (EventoService eventoService) =>
{
    return Results.Ok(eventoService.ObtenerTodos());
})
.WithName("ObtenerEventos")
.WithTags("Eventos")
.WithSummary("Lista los eventos disponibles.");

app.MapGet("/api/eventos/{id:guid}", (Guid id, EventoService eventoService) =>
{
    var evento = eventoService.ObtenerPorId(id);
    return evento != null ? Results.Ok(evento) : Results.NotFound(new { mensaje = $"Evento con ID '{id}' no encontrado." });
})
.WithName("ObtenerEventoPorId")
.WithTags("Eventos")
.WithSummary("Detalle de un evento con sus modalidades de entrada.");

app.MapPost("/api/eventos", (HttpContext context, CrearEventoDto dto, EventoService eventoService, UsuarioService usuarioService) =>
{
    try
    {
        string? dni = ObtenerDni(context);
        usuarioService.ValidarRol(dni, RolUsuario.Organizador);

        var nuevoEvento = eventoService.Crear(dto.Nombre, dto.Descripcion, dto.Fecha, dto.Lugar, dto.Latitud, dto.Longitud);
        return Results.Created($"/api/eventos/{nuevoEvento.Id}", nuevoEvento);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CrearEvento")
.WithTags("Eventos")
.WithSummary("Crea un evento (Requiere rol Organizador).");

app.MapPut("/api/eventos/{id:guid}", (HttpContext context, Guid id, ModificarEventoDto dto, EventoService eventoService, UsuarioService usuarioService) =>
{
    try
    {
        string? dni = ObtenerDni(context);
        usuarioService.ValidarRol(dni, RolUsuario.Organizador);

        var eventoModificado = eventoService.Modificar(id, dto.Nombre, dto.Descripcion, dto.Fecha, dto.Lugar, dto.Latitud, dto.Longitud);
        return Results.Ok(eventoModificado);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("ModificarEvento")
.WithTags("Eventos")
.WithSummary("Edita un evento existente (Requiere rol Organizador).");

app.MapPut("/api/eventos/{id:guid}/cancelar", (HttpContext context, Guid id, EventoService eventoService, UsuarioService usuarioService) =>
{
    try
    {
        string? dni = ObtenerDni(context);
        usuarioService.ValidarRol(dni, RolUsuario.Organizador);

        var eventoCancelado = eventoService.Cancelar(id);
        return Results.Ok(new { mensaje = "Evento cancelado exitosamente.", evento = eventoCancelado });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
})
.WithName("CancelarEvento")
.WithTags("Eventos")
.WithSummary("Cancela un evento (Requiere rol Organizador).");

app.MapPost("/api/eventos/{id:guid}/modalidades", (HttpContext context, Guid id, CrearModalidadDto dto, EventoService eventoService, UsuarioService usuarioService) =>
{
    try
    {
        string? dni = ObtenerDni(context);
        usuarioService.ValidarRol(dni, RolUsuario.Organizador);

        var modalidad = eventoService.AgregarModalidad(id, dto.Nombre, dto.Precio, dto.Beneficios, dto.CupoMaximo);
        return Results.Created($"/api/eventos/{id}/modalidades/{modalidad.Id}", modalidad);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("AgregarModalidad")
.WithTags("Eventos")
.WithSummary("Agrega una modalidad de entrada a un evento (Requiere rol Organizador).");

// -------------------------------------------------------------
// 3. Compras
// -------------------------------------------------------------
app.MapPost("/api/compras", (HttpContext context, RealizarCompraDto dto, CompraService compraService, UsuarioService usuarioService) =>
{
    try
    {
        string dni = !string.IsNullOrWhiteSpace(dto.DniComprador) ? dto.DniComprador : (ObtenerDni(context) ?? string.Empty);
        usuarioService.ValidarRol(dni, RolUsuario.Comprador);

        var compra = compraService.RealizarCompra(dni, dto.IdEvento, dto.IdModalidad, dto.Cantidad);
        return Results.Created($"/api/compras/{compra.Id}", compra);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("RegistrarCompra")
.WithTags("Compras")
.WithSummary("Registra una compra y genera las entradas individuales (Requiere rol Comprador).");

app.MapGet("/api/compras/{id:guid}", (Guid id, CompraService compraService) =>
{
    var compra = compraService.ObtenerPorId(id);
    return compra != null ? Results.Ok(compra) : Results.NotFound(new { mensaje = $"Compra con ID '{id}' no encontrada." });
})
.WithName("ObtenerCompraPorId")
.WithTags("Compras")
.WithSummary("Consulta el detalle de una compra y el estado de cada entrada que generó.");

app.MapGet("/api/compras", (HttpContext context, CompraService compraService) =>
{
    string? dni = ObtenerDni(context);
    if (!string.IsNullOrWhiteSpace(dni))
    {
        return Results.Ok(compraService.ObtenerPorDni(dni));
    }
    return Results.Ok(compraService.ObtenerTodas());
})
.WithName("ObtenerCompras")
.WithTags("Compras")
.WithSummary("Consulta compras (filtradas por DNI o todas).");

// -------------------------------------------------------------
// 4. Reportes
// -------------------------------------------------------------
app.MapGet("/api/reportes/recaudacion", (HttpContext context, EventoService eventoService, UsuarioService usuarioService) =>
{
    try
    {
        string? dni = ObtenerDni(context);
        usuarioService.ValidarRol(dni, RolUsuario.Organizador);

        var reporte = eventoService.ObtenerReporteRecaudacion();
        return Results.Ok(reporte);
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
    }
})
.WithName("ReporteRecaudacion")
.WithTags("Reportes")
.WithSummary("Recaudación y entradas vendidas por evento (Requiere rol Organizador).");

// -------------------------------------------------------------
// 5. Cancelación de Entrada (Promoción)
// -------------------------------------------------------------
app.MapDelete("/api/entradas/{codigo}", (HttpContext context, string codigo, CompraService compraService, UsuarioService usuarioService) =>
{
    try
    {
        string? dni = ObtenerDni(context);
        usuarioService.ValidarRol(dni, RolUsuario.Comprador);

        var entradaCancelada = compraService.CancelarEntrada(codigo, dni!);
        return Results.Ok(new { mensaje = "Entrada cancelada con éxito. El cupo ha sido restablecido.", entrada = entradaCancelada });
    }
    catch (UnauthorizedAccessException ex)
    {
        return Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status403Forbidden);
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { error = ex.Message });
    }
    catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CancelarEntrada")
.WithTags("Entradas")
.WithSummary("Cancela una entrada comprada y restablece el cupo (Promoción - Requiere rol Comprador).");

app.Run();

// DTOs auxiliares para requests
public record CrearEventoDto(string Nombre, string Descripcion, DateTime Fecha, string Lugar, double? Latitud, double? Longitud);
public record ModificarEventoDto(string Nombre, string Descripcion, DateTime Fecha, string Lugar, double? Latitud, double? Longitud);
public record CrearModalidadDto(string Nombre, decimal Precio, string Beneficios, int CupoMaximo);
public record RealizarCompraDto(string? DniComprador, Guid IdEvento, Guid IdModalidad, int Cantidad);
