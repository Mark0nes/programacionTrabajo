using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ValidacionEventos.Logica;

namespace Testing;

[TestFixture]
public class ValidacionEntradasTests
{
    private string _testDir = null!;
    private string _comprasFile = null!;
    private string _eventosFile = null!;
    private ValidadorEntradaService _validador = null!;

    private Guid _evento1Id;
    private Guid _evento2Id;
    private const string CodigoValido = "VAL123";
    private const string CodigoUsado = "USD456";
    private const string CodigoEvento2 = "EVT789";
    private const string CodigoCancelado = "CNC000";

    [SetUp]
    public void Setup()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "TestValidacion_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);

        _comprasFile = Path.Combine(_testDir, "compras.json");
        _eventosFile = Path.Combine(_testDir, "eventos.json");

        _evento1Id = Guid.NewGuid();
        _evento2Id = Guid.NewGuid();

        var eventos = new JArray
        {
            new JObject
            {
                ["Id"] = _evento1Id.ToString(),
                ["Nombre"] = "Lollapalooza 2026",
                ["Cancelado"] = false,
                ["Fecha"] = DateTime.Now.AddDays(10)
            },
            new JObject
            {
                ["Id"] = _evento2Id.ToString(),
                ["Nombre"] = "Quilmes Rock 2026",
                ["Cancelado"] = false,
                ["Fecha"] = DateTime.Now.AddDays(15)
            }
        };
        File.WriteAllText(_eventosFile, eventos.ToString(Formatting.Indented));

        var compras = new JArray
        {
            new JObject
            {
                ["Id"] = Guid.NewGuid().ToString(),
                ["DniComprador"] = "40123456",
                ["IdEvento"] = _evento1Id.ToString(),
                ["NombreEvento"] = "Lollapalooza 2026",
                ["Entradas"] = new JArray
                {
                    new JObject
                    {
                        ["Codigo"] = CodigoValido,
                        ["IdEvento"] = _evento1Id.ToString(),
                        ["NombreEvento"] = "Lollapalooza 2026",
                        ["NombreModalidad"] = "Campo General",
                        ["Usada"] = false,
                        ["FechaUso"] = null,
                        ["Cancelada"] = false
                    },
                    new JObject
                    {
                        ["Codigo"] = CodigoUsado,
                        ["IdEvento"] = _evento1Id.ToString(),
                        ["NombreEvento"] = "Lollapalooza 2026",
                        ["NombreModalidad"] = "Campo VIP",
                        ["Usada"] = true,
                        ["FechaUso"] = DateTime.Now.AddHours(-2),
                        ["Cancelada"] = false
                    },
                    new JObject
                    {
                        ["Codigo"] = CodigoCancelado,
                        ["IdEvento"] = _evento1Id.ToString(),
                        ["NombreEvento"] = "Lollapalooza 2026",
                        ["NombreModalidad"] = "Campo General",
                        ["Usada"] = false,
                        ["FechaUso"] = null,
                        ["Cancelada"] = true
                    }
                }
            },
            new JObject
            {
                ["Id"] = Guid.NewGuid().ToString(),
                ["DniComprador"] = "38456789",
                ["IdEvento"] = _evento2Id.ToString(),
                ["NombreEvento"] = "Quilmes Rock 2026",
                ["Entradas"] = new JArray
                {
                    new JObject
                    {
                        ["Codigo"] = CodigoEvento2,
                        ["IdEvento"] = _evento2Id.ToString(),
                        ["NombreEvento"] = "Quilmes Rock 2026",
                        ["NombreModalidad"] = "Platea Alta",
                        ["Usada"] = false,
                        ["FechaUso"] = null,
                        ["Cancelada"] = false
                    }
                }
            }
        };
        File.WriteAllText(_comprasFile, compras.ToString(Formatting.Indented));

        _validador = new ValidadorEntradaService(_comprasFile, _eventosFile);
    }

    [TearDown]
    public void Cleanup()
    {
        if (Directory.Exists(_testDir))
        {
            try
            {
                Directory.Delete(_testDir, true);
            }
            catch { }
        }
    }

    [Test]
    public void ValidarEntrada_EntradaValida_AutorizaIngresoYMarcaComoUsada()
    {
        var resultado = _validador.ValidarEntrada(CodigoValido, _evento1Id);

        Assert.That(resultado.Exitoso, Is.True);
        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.Valida));
        Assert.That(resultado.Mensaje, Does.Contain("Ingreso autorizado"));
        Assert.That(resultado.FechaUso, Is.Not.Null);

        string json = File.ReadAllText(_comprasFile);
        var compras = JArray.Parse(json);
        var entrada = compras[0]["Entradas"]!.First(e => e["Codigo"]!.ToString() == CodigoValido);
        Assert.That(entrada["Usada"]!.Value<bool>(), Is.True);
    }

    [Test]
    public void ValidarEntrada_EntradaYaUsada_RechazaConEstadoYaFueUsada()
    {
        var resultado = _validador.ValidarEntrada(CodigoUsado, _evento1Id);

        Assert.That(resultado.Exitoso, Is.False);
        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.YaFueUsada));
        Assert.That(resultado.Mensaje, Does.Contain("ya fue utilizada"));
    }

    [Test]
    public void ValidarEntrada_EntradaInexistente_RechazaConEstadoNoExiste()
    {
        var resultado = _validador.ValidarEntrada("NOEXISTE", _evento1Id);

        Assert.That(resultado.Exitoso, Is.False);
        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.NoExiste));
        Assert.That(resultado.Mensaje, Does.Contain("no existe"));
    }

    [Test]
    public void ValidarEntrada_EntradaDeOtroEvento_RechazaConEstadoEventoIncorrecto()
    {
        var resultado = _validador.ValidarEntrada(CodigoEvento2, _evento1Id);

        Assert.That(resultado.Exitoso, Is.False);
        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoIncorrecto));
        Assert.That(resultado.Mensaje, Does.Contain("no corresponde a este evento"));
    }

    [Test]
    public void ValidarEntrada_EntradaCanceladaPorComprador_RechazaIngreso()
    {
        var resultado = _validador.ValidarEntrada(CodigoCancelado, _evento1Id);

        Assert.That(resultado.Exitoso, Is.False);
        Assert.That(resultado.Mensaje, Does.Contain("cancelada"));
    }

    [Test]
    public void ValidarEntrada_EventoCancelado_RechazaIngreso()
    {
        string json = File.ReadAllText(_eventosFile);
        var eventos = JArray.Parse(json);
        eventos[0]["Cancelado"] = true;
        File.WriteAllText(_eventosFile, eventos.ToString(Formatting.Indented));

        var resultado = _validador.ValidarEntrada(CodigoValido, _evento1Id);

        Assert.That(resultado.Exitoso, Is.False);
        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoCancelado));
        Assert.That(resultado.Mensaje, Does.Contain("cancelado"));
    }

    [Test]
    public void ValidarEntrada_DobleValidacionConsecutiva_SegundaFallaPorYaUsada()
    {
        var res1 = _validador.ValidarEntrada(CodigoValido, _evento1Id);
        Assert.That(res1.Exitoso, Is.True);

        var res2 = _validador.ValidarEntrada(CodigoValido, _evento1Id);
        Assert.That(res2.Exitoso, Is.False);
        Assert.That(res2.Estado, Is.EqualTo(EstadoValidacion.YaFueUsada));
    }
}
