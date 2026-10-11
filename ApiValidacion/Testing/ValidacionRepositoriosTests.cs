using Newtonsoft.Json.Linq;
using ValidacionEventos.Data;
using ValidacionEventos.Logica;

namespace Testing;

/// <summary>
/// Pruebas con archivos reales (temporales, uno por test) usando el formato exacto que escribe la API de
/// Gestión: verifican que Validación lee ese formato y que al reescribirlo no pierde ningún campo.
/// </summary>
[TestFixture]
public class ValidacionRepositoriosTests
{
    private const string IdEvento = "a1111111-1111-1111-1111-111111111111";

    private const string ComprasDeGestion = @"[
  {
    ""Id"": ""93f68ec3-cfb2-4bcd-b63e-573334355190"",
    ""DniComprador"": ""40123456"",
    ""FechaCompra"": ""2026-10-01T19:18:42.7691111-03:00"",
    ""IdEvento"": ""a1111111-1111-1111-1111-111111111111"",
    ""NombreEvento"": ""Festival"",
    ""IdModalidad"": ""b1111111-1111-1111-1111-111111111111"",
    ""NombreModalidad"": ""General"",
    ""Cantidad"": 2,
    ""PrecioUnitario"": 1000.0,
    ""Total"": 2000.0,
    ""Entradas"": [
      { ""Codigo"": ""AAA111"", ""IdEvento"": ""a1111111-1111-1111-1111-111111111111"", ""NombreEvento"": ""Festival"",
        ""IdModalidad"": ""b1111111-1111-1111-1111-111111111111"", ""NombreModalidad"": ""General"",
        ""IdCompra"": ""93f68ec3-cfb2-4bcd-b63e-573334355190"", ""PrecioUnitario"": 1000.0,
        ""Usada"": false, ""FechaUso"": null, ""Cancelada"": false },
      { ""Codigo"": ""BBB222"", ""IdEvento"": ""a1111111-1111-1111-1111-111111111111"", ""NombreEvento"": ""Festival"",
        ""IdModalidad"": ""b1111111-1111-1111-1111-111111111111"", ""NombreModalidad"": ""General"",
        ""IdCompra"": ""93f68ec3-cfb2-4bcd-b63e-573334355190"", ""PrecioUnitario"": 1000.0,
        ""Usada"": false, ""FechaUso"": null, ""Cancelada"": false }
    ]
  }
]";

    private string _carpeta = string.Empty;
    private string _rutaCompras = string.Empty;
    private string _rutaEventos = string.Empty;

    [SetUp]
    public void Preparar()
    {
        _carpeta = Path.Combine(Path.GetTempPath(), "TestValidacion_" + Guid.NewGuid());
        Directory.CreateDirectory(_carpeta);
        _rutaCompras = Path.Combine(_carpeta, "compras.json");
        _rutaEventos = Path.Combine(_carpeta, "eventos.json");
        File.WriteAllText(_rutaCompras, ComprasDeGestion);
    }

    [TearDown]
    public void Limpiar()
    {
        if (Directory.Exists(_carpeta))
        {
            Directory.Delete(_carpeta, true);
        }
    }

    [Test]
    public void ObtenerCompras_ArchivoGeneradoPorGestion_CargaLasEntradas()
    {
        var repositorio = new ValidacionCompraRepository(_rutaCompras);

        var compras = repositorio.ObtenerCompras();

        Assert.That(compras[0].Entradas.Select(e => e.Codigo), Is.EqualTo(new[] { "AAA111", "BBB222" }));
    }

    [Test]
    public void ObtenerCompras_ArchivoInexistente_DevuelveListaVacia()
    {
        var repositorio = new ValidacionCompraRepository(Path.Combine(_carpeta, "no_existe.json"));

        Assert.That(repositorio.ObtenerCompras(), Is.Empty);
    }

    [Test]
    public void ObtenerCompras_ArchivoVacio_DevuelveListaVacia()
    {
        File.WriteAllText(_rutaCompras, "");
        var repositorio = new ValidacionCompraRepository(_rutaCompras);

        Assert.That(repositorio.ObtenerCompras(), Is.Empty);
    }

    [Test]
    public void ValidarEntrada_ConArchivosReales_QuedaUsadaEnElArchivo()
    {
        var validador = new ValidadorEntradaService(new ValidacionCompraRepository(_rutaCompras), new ValidacionEventoRepository(_rutaEventos));

        validador.ValidarEntrada("AAA111", Guid.Parse(IdEvento));

        var compras = JArray.Parse(File.ReadAllText(_rutaCompras));
        Assert.That(compras[0]["Entradas"]![0]!["Usada"]!.Value<bool>(), Is.True);
    }

    [Test]
    public void ValidarEntrada_ConArchivosReales_NoMarcaLaOtraEntradaDeLaCompra()
    {
        var validador = new ValidadorEntradaService(new ValidacionCompraRepository(_rutaCompras), new ValidacionEventoRepository(_rutaEventos));

        validador.ValidarEntrada("AAA111", Guid.Parse(IdEvento));

        var compras = JArray.Parse(File.ReadAllText(_rutaCompras));
        Assert.That(compras[0]["Entradas"]![1]!["Usada"]!.Value<bool>(), Is.False);
    }

    [Test]
    public void ValidarEntrada_ConArchivosReales_ConservaElTotalQueEscribioGestion()
    {
        var validador = new ValidadorEntradaService(new ValidacionCompraRepository(_rutaCompras), new ValidacionEventoRepository(_rutaEventos));

        validador.ValidarEntrada("AAA111", Guid.Parse(IdEvento));

        var compras = JArray.Parse(File.ReadAllText(_rutaCompras));
        Assert.That(compras[0]["Total"]!.Value<decimal>(), Is.EqualTo(2000m));
    }

    [Test]
    public void ValidarEntrada_ConArchivosReales_ConservaPrecioYDniDeLaCompra()
    {
        var validador = new ValidadorEntradaService(new ValidacionCompraRepository(_rutaCompras), new ValidacionEventoRepository(_rutaEventos));

        validador.ValidarEntrada("AAA111", Guid.Parse(IdEvento));

        var compra = JArray.Parse(File.ReadAllText(_rutaCompras))[0];
        Assert.Multiple(() =>
        {
            Assert.That(compra["DniComprador"]!.ToString(), Is.EqualTo("40123456"));
            Assert.That(compra["PrecioUnitario"]!.Value<decimal>(), Is.EqualTo(1000m));
            Assert.That(compra["NombreModalidad"]!.ToString(), Is.EqualTo("General"));
        });
    }

    [Test]
    public void ValidarEntrada_ReiniciandoLosRepositorios_LaEntradaSigueUsada()
    {
        var primero = new ValidadorEntradaService(new ValidacionCompraRepository(_rutaCompras), new ValidacionEventoRepository(_rutaEventos));
        primero.ValidarEntrada("AAA111", Guid.Parse(IdEvento));

        // Equivale a reiniciar la API: instancias nuevas que leen el archivo.
        var reiniciado = new ValidadorEntradaService(new ValidacionCompraRepository(_rutaCompras), new ValidacionEventoRepository(_rutaEventos));
        var resultado = reiniciado.ValidarEntrada("AAA111", Guid.Parse(IdEvento));

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.YaFueUsada));
    }

    [Test]
    public void ObtenerPorId_EventoCanceladoEnElArchivo_LoCargaCancelado()
    {
        File.WriteAllText(_rutaEventos, "[ { \"Id\": \"" + IdEvento + "\", \"Nombre\": \"Festival\", \"Fecha\": \"2026-10-15T18:00:00\", \"Lugar\": \"Parque\", \"Cancelado\": true, \"Modalidades\": [] } ]");
        var repositorio = new ValidacionEventoRepository(_rutaEventos);

        var evento = repositorio.ObtenerPorId(Guid.Parse(IdEvento));

        Assert.That(evento!.Cancelado, Is.True);
    }

    [Test]
    public void ValidarEntrada_EventoCanceladoEnElArchivo_RechazaElIngreso()
    {
        File.WriteAllText(_rutaEventos, "[ { \"Id\": \"" + IdEvento + "\", \"Nombre\": \"Festival\", \"Fecha\": \"2026-10-15T18:00:00\", \"Lugar\": \"Parque\", \"Cancelado\": true, \"Modalidades\": [] } ]");
        var validador = new ValidadorEntradaService(new ValidacionCompraRepository(_rutaCompras), new ValidacionEventoRepository(_rutaEventos));

        var resultado = validador.ValidarEntrada("AAA111", Guid.Parse(IdEvento));

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoCancelado));
    }
}
