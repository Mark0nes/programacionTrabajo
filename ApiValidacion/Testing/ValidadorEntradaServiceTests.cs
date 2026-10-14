using ValidacionEventos.Dominio;
using ValidacionEventos.Logica;

namespace Testing;

[TestFixture]
public class ValidadorEntradaServiceTests
{
    private static readonly DateTime Ahora = new DateTime(2026, 10, 20, 21, 30, 0);

    private CompraRepositorioFalso _compras = null!;
    private EventoRepositorioFalso _eventos = null!;
    private ValidadorEntradaService _validador = null!;

    private Guid _idEvento1;
    private Guid _idEvento2;

    [SetUp]
    public void Preparar()
    {
        _idEvento1 = Guid.NewGuid();
        _idEvento2 = Guid.NewGuid();

        _eventos = new EventoRepositorioFalso();
        _eventos.Eventos.Add(new Evento(_idEvento1, "Lollapalooza", Ahora, "Hipódromo"));
        _eventos.Eventos.Add(new Evento(_idEvento2, "Quilmes Rock", Ahora, "Estadio"));

        _compras = new CompraRepositorioFalso();
        _validador = new ValidadorEntradaService(_compras, _eventos, () => Ahora);
    }

    /// <summary>Agrega una compra del evento 1 con las entradas indicadas y las devuelve.</summary>
    private List<Entrada> AgregarCompra(Guid idEvento, params string[] codigos)
    {
        string nombre = idEvento == _idEvento1 ? "Lollapalooza" : "Quilmes Rock";
        var compra = new Compra("40123456", idEvento, nombre, Guid.NewGuid(), "General", codigos.Length, 1000m, Ahora);

        foreach (string codigo in codigos)
        {
            compra.AgregarEntrada(new Entrada(codigo, idEvento, nombre, compra.IdModalidad, "General", compra.Id, 1000m));
        }

        _compras.Compras.Add(compra);
        return compra.Entradas;
    }

    // ---------- Entrada válida ----------

    [Test]
    public void ValidarEntrada_EntradaValida_AutorizaElIngreso()
    {
        AgregarCompra(_idEvento1, "VAL123");

        var resultado = _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(resultado.Exitoso, Is.True);
    }

    [Test]
    public void ValidarEntrada_EntradaValida_QuedaMarcadaComoUsada()
    {
        var entradas = AgregarCompra(_idEvento1, "VAL123");

        _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(entradas[0].Usada, Is.True);
    }

    // ---------- Uso único ----------

    [Test]
    public void ValidarEntrada_SegundaValidacionDeLaMismaEntrada_RechazaPorYaUsada()
    {
        AgregarCompra(_idEvento1, "VAL123");
        _validador.ValidarEntrada("VAL123", _idEvento1);

        var segunda = _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(segunda.Estado, Is.EqualTo(EstadoValidacion.YaFueUsada));
    }

    // ---------- Cada entrada se valida por separado ----------

    [Test]
    public void ValidarEntrada_CompraDeTresEntradas_ValidarUnaNoMarcaLasOtras()
    {
        var entradas = AgregarCompra(_idEvento1, "AAA111", "BBB222", "CCC333");

        _validador.ValidarEntrada("AAA111", _idEvento1);

        Assert.That(entradas.Count(e => e.Usada), Is.EqualTo(1));
    }

    // ---------- Rechazos ----------

    [Test]
    public void ValidarEntrada_CodigoInexistente_RechazaConEstadoNoExiste()
    {
        AgregarCompra(_idEvento1, "VAL123");

        var resultado = _validador.ValidarEntrada("NOEXIS", _idEvento1);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.NoExiste));
    }

    [Test]
    public void ValidarEntrada_CodigoVacio_RechazaSinAutorizar()
    {
        var resultado = _validador.ValidarEntrada("   ", _idEvento1);

        Assert.That(resultado.Exitoso, Is.False);
    }

    [Test]
    public void ValidarEntrada_EntradaDeOtroEvento_RechazaConEstadoEventoIncorrecto()
    {
        AgregarCompra(_idEvento2, "EVT789");

        var resultado = _validador.ValidarEntrada("EVT789", _idEvento1);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoIncorrecto));
    }

    [Test]
    public void ValidarEntrada_SinEvento_RechazaConEstadoEventoIncorrecto()
    {
        AgregarCompra(_idEvento1, "VAL123");

        var resultado = _validador.ValidarEntrada("VAL123", Guid.Empty);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoIncorrecto));
    }

    [Test]
    public void ValidarEntrada_EntradaCancelada_RechazaConEstadoCancelada()
    {
        AgregarCompraConEntradaCancelada("CNC000");

        var resultado = _validador.ValidarEntrada("CNC000", _idEvento1);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.Cancelada));
    }

    [Test]
    public void ValidarEntrada_EventoCancelado_RechazaConEstadoEventoCancelado()
    {
        AgregarCompra(_idEvento1, "VAL123");
        _eventos.Eventos[0] = new Evento(_idEvento1, "Lollapalooza", Ahora, "Hipódromo", cancelado: true);

        var resultado = _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoCancelado));
    }

    private List<Entrada> AgregarCompraConEntradaCancelada(string codigo)
    {
        var entradas = AgregarCompra(_idEvento1, codigo);
        // La cancelación la hace Gestión; acá solo se reconstruye el estado que deja en el archivo.
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(_compras.Compras);
        json = json.Replace("\"Cancelada\":false", "\"Cancelada\":true");
        _compras.Compras = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Compra>>(json)!;
        return _compras.Compras[0].Entradas;
    }
}
