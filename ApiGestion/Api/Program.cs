using GestionEventos.Data;
using GestionEventos.Logica;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<PersonaRepository>();
builder.Services.AddSingleton<EventoRepository>();
builder.Services.AddSingleton<CompraRepository>();
builder.Services.AddSingleton<EntradaRepository>();
builder.Services.AddSingleton<UsuarioService>();
builder.Services.AddSingleton<EventoService>();
builder.Services.AddSingleton<CompraService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gestión v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

var frontendPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "CarpetaFrontend"));
if (!Directory.Exists(frontendPath))
{
    frontendPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "CarpetaFrontend"));
}

if (Directory.Exists(frontendPath))
{
    app.UseDefaultFiles();
    app.UseStaticFiles(new Microsoft.AspNetCore.Builder.StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });

    app.MapGet("/", () => Results.File(Path.Combine(frontendPath, "index.html"), "text/html"));
    app.MapGet("/eventos", () => Results.File(Path.Combine(frontendPath, "eventos.html"), "text/html"));
    app.MapGet("/comprar-entrada", () => Results.File(Path.Combine(frontendPath, "comprar-entrada.html"), "text/html"));
    app.MapGet("/consultar-compra", () => Results.File(Path.Combine(frontendPath, "consultar-compra.html"), "text/html"));
    app.MapGet("/control-acceso", () => Results.File(Path.Combine(frontendPath, "control-acceso.html"), "text/html"));
    app.MapGet("/gestion-eventos", () => Results.File(Path.Combine(frontendPath, "gestion-eventos.html"), "text/html"));
    app.MapGet("/reporte-recaudacion", () => Results.File(Path.Combine(frontendPath, "reporte-recaudacion.html"), "text/html"));
}

app.MapControllers();

app.Run();
