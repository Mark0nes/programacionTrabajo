using Microsoft.OpenApi.Models;
using ValidacionEventos.Logica;

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
        Title = "API Validación de Entradas en Puerta",
        Version = "v1",
        Description = "API utilizada el día del evento en la puerta de acceso para validar entradas individuales por código alfanumérico."
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

builder.Services.AddSingleton<ValidadorEntradaService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Validación v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

// -------------------------------------------------------------
// Endpoint de validación de entradas
// -------------------------------------------------------------
app.MapPost("/api/validaciones", (ValidarEntradaDto dto, ValidadorEntradaService validador) =>
{
    if (string.IsNullOrWhiteSpace(dto.Codigo))
    {
        return Results.BadRequest(new ResultadoValidacion
        {
            Exitoso = false,
            Estado = EstadoValidacion.NoExiste,
            Mensaje = "El código de entrada es obligatorio."
        });
    }

    var resultado = validador.ValidarEntrada(dto.Codigo, dto.IdEvento);

    if (resultado.Exitoso)
    {
        return Results.Ok(resultado);
    }

    // Si falló por cualquiera de las razones (no existe, ya fue usada, otro evento, evento cancelado)
    return Results.BadRequest(resultado);
})
.WithName("ValidarEntrada")
.WithTags("Validaciones")
.WithSummary("Valida una entrada para ingresar al evento y la marca como usada si es válida.");

app.Run();

public record ValidarEntradaDto(string Codigo, Guid? IdEvento);
