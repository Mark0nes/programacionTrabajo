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
    public void ValidarEntrada_EntradaValida_DevuelveEstadoValida()
    {
        AgregarCompra(_idEvento1, "VAL123");

        var resultado = _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.Valida));
    }

    [Test]
    public void ValidarEntrada_EntradaValida_QuedaMarcadaComoUsada()
    {
        var entradas = AgregarCompra(_idEvento1, "VAL123");

        _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(entradas[0].Usada, Is.True);
    }

    [Test]
    public void ValidarEntrada_EntradaValida_RegistraLaFechaYHoraDelIngreso()
    {
        var entradas = AgregarCompra(_idEvento1, "VAL123");

        _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(entradas[0].FechaUso, Is.EqualTo(Ahora));
    }

    [Test]
    public void ValidarEntrada_EntradaValida_GuardaElCambioEnElRepositorio()
    {
        AgregarCompra(_idEvento1, "VAL123");

        _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(_compras.VecesGuardado, Is.EqualTo(1));
    }

    [Test]
    public void ValidarEntrada_CodigoEnMinusculasYConEspacios_LoValidaIgual()
    {
        AgregarCompra(_idEvento1, "VAL123");

        var resultado = _validador.ValidarEntrada("  val123 ", _idEvento1);

        Assert.That(resultado.Exitoso, Is.True);
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

    [Test]
    public void ValidarEntrada_SegundaValidacionDeLaMismaEntrada_NoAutorizaElIngreso()
    {
        AgregarCompra(_idEvento1, "VAL123");
        _validador.ValidarEntrada("VAL123", _idEvento1);

        var segunda = _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(segunda.Exitoso, Is.False);
    }

    [Test]
    public void ValidarEntrada_EntradaYaUsada_InformaCuandoSeUso()
    {
        var entradas = AgregarCompra(_idEvento1, "USD456");
        entradas[0].MarcarComoUsada(Ahora.AddHours(-2));

        var resultado = _validador.ValidarEntrada("USD456", _idEvento1);

        Assert.That(resultado.FechaUso, Is.EqualTo(Ahora.AddHours(-2)));
    }

    [Test]
    public void ValidarEntrada_EntradaYaUsada_NoVuelveAGuardar()
    {
        var entradas = AgregarCompra(_idEvento1, "USD456");
        entradas[0].MarcarComoUsada(Ahora.AddHours(-2));

        _validador.ValidarEntrada("USD456", _idEvento1);

        Assert.That(_compras.VecesGuardado, Is.EqualTo(0));
    }

    // ---------- Cada entrada se valida por separado ----------

    [Test]
    public void ValidarEntrada_CompraDeTresEntradas_ValidarUnaNoMarcaLasOtras()
    {
        var entradas = AgregarCompra(_idEvento1, "AAA111", "BBB222", "CCC333");

        _validador.ValidarEntrada("AAA111", _idEvento1);

        Assert.That(entradas.Count(e => e.Usada), Is.EqualTo(1));
    }

    [Test]
    public void ValidarEntrada_CompraDeTresEntradas_CadaUnaPuedeIngresarPorSeparado()
    {
        AgregarCompra(_idEvento1, "AAA111", "BBB222", "CCC333");

        var resultados = new[] { "AAA111", "BBB222", "CCC333" }
            .Select(codigo => _validador.ValidarEntrada(codigo, _idEvento1))
            .ToList();

        Assert.That(resultados.All(r => r.Exitoso), Is.True);
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
    public void ValidarEntrada_CodigoNulo_RechazaSinAutorizar()
    {
        var resultado = _validador.ValidarEntrada(null, _idEvento1);

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
    public void ValidarEntrada_EntradaDeOtroEvento_NoLaMarcaComoUsada()
    {
        var entradas = AgregarCompra(_idEvento2, "EVT789");

        _validador.ValidarEntrada("EVT789", _idEvento1);

        Assert.That(entradas[0].Usada, Is.False);
    }

    [Test]
    public void ValidarEntrada_SinEvento_RechazaConEstadoEventoIncorrecto()
    {
        AgregarCompra(_idEvento1, "VAL123");

        var resultado = _validador.ValidarEntrada("VAL123", Guid.Empty);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoIncorrecto));
    }

    [Test]
    public void ValidarEntrada_SinEvento_NoMarcaLaEntradaComoUsada()
    {
        var entradas = AgregarCompra(_idEvento1, "VAL123");

        _validador.ValidarEntrada("VAL123", Guid.Empty);

        Assert.That(entradas[0].Usada, Is.False);
    }

    [Test]
    public void ValidarEntrada_EntradaCancelada_RechazaConEstadoCancelada()
    {
        AgregarCompraConEntradaCancelada("CNC000");

        var resultado = _validador.ValidarEntrada("CNC000", _idEvento1);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.Cancelada));
    }

    [Test]
    public void ValidarEntrada_EntradaCancelada_NoLaMarcaComoUsada()
    {
        var entradas = AgregarCompraConEntradaCancelada("CNC000");

        _validador.ValidarEntrada("CNC000", _idEvento1);

        Assert.That(entradas[0].Usada, Is.False);
    }

    [Test]
    public void ValidarEntrada_EntradaCanceladaDeOtroEvento_InformaEventoIncorrecto()
    {
        AgregarCompraConEntradaCancelada("CNC000");

        var resultado = _validador.ValidarEntrada("CNC000", _idEvento2);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoIncorrecto));
    }

    [Test]
    public void ValidarEntrada_EventoCancelado_RechazaConEstadoEventoCancelado()
    {
        AgregarCompra(_idEvento1, "VAL123");
        _eventos.Eventos[0] = new Evento(_idEvento1, "Lollapalooza", Ahora, "Hipódromo", cancelado: true);

        var resultado = _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(resultado.Estado, Is.EqualTo(EstadoValidacion.EventoCancelado));
    }

    [Test]
    public void ValidarEntrada_EventoCancelado_NoMarcaLaEntradaComoUsada()
    {
        var entradas = AgregarCompra(_idEvento1, "VAL123");
        _eventos.Eventos[0] = new Evento(_idEvento1, "Lollapalooza", Ahora, "Hipódromo", cancelado: true);

        _validador.ValidarEntrada("VAL123", _idEvento1);

        Assert.That(entradas[0].Usada, Is.False);
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
