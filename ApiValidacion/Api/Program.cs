using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using ValidacionEventos.Api.DTOs;
using ValidacionEventos.Data;
using ValidacionEventos.Logica;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Pedido con formato inválido (falta el código o el evento): 400 con el mismo formato de respuesta que la validación.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = contexto =>
    {
        var mensajes = contexto.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Datos inválidos." : e.ErrorMessage);
        return new BadRequestObjectResult(
            new ValidacionResponse(false, "DatosInvalidos", string.Join(" ", mensajes), null, null, null, null));
    };
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<IValidacionCompraRepository, ValidacionCompraRepository>();
builder.Services.AddSingleton<IValidacionEventoRepository, ValidacionEventoRepository>();
builder.Services.AddSingleton<ValidadorEntradaService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Validación v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

app.MapControllers();

app.Run();
