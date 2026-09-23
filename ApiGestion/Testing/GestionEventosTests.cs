using GestionEventos.Data;
using GestionEventos.Logica;

namespace Testing;

[TestFixture]
public class GestionEventosTests
{
    private string _testDir = null!;
    private EventoRepository _eventoRepo = null!;
    private CompraRepository _compraRepo = null!;
    private PersonaRepository _personaRepo = null!;
    private UsuarioService _usuarioService = null!;
    private EventoService _eventoService = null!;
    private CompraService _compraService = null!;

    [SetUp]
    public void Setup()
    {
        _testDir = Path.Combine(Path.GetTempPath(), "TestGestion_" + Guid.NewGuid());
        Directory.CreateDirectory(_testDir);

        string usuariosFile = Path.Combine(_testDir, "usuarios.json");
        string eventosFile = Path.Combine(_testDir, "eventos.json");
        string comprasFile = Path.Combine(_testDir, "compras.json");

        _personaRepo = new PersonaRepository(usuariosFile);
        _eventoRepo = new EventoRepository(eventosFile);
        _compraRepo = new CompraRepository(comprasFile);

        // Preload sample users
        var usuarios = new List<Usuario>
        {
            new("30111222", "Lucía Fernández", "lfernandez", RolUsuario.Organizador),
            new("40123456", "Sofía Gómez", "sgomez", RolUsuario.Comprador),
            new("38456789", "Nicolás Pereyra", "npereyra", RolUsuario.Comprador)
        };
        _personaRepo.GuardarUsuarios(usuarios);

        _usuarioService = new UsuarioService(_personaRepo);
        _eventoService = new EventoService(_eventoRepo, _compraRepo);
        _compraService = new CompraService(_compraRepo, _eventoRepo, _usuarioService);
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
            catch
            {
                // Ignorar error al limpiar directorio temporal
            }
        }
    }

    [Test]
    public void DescuentoPorVolumen_CompraDeCincoOMasEntradas_AplicaDescuentoDel15PorCiento()
    {
        var modalidad = new Modalidad("General", 1000m, "Acceso general", 50);

        // 4 entradas -> precio normal 4000
        decimal precioCuatro = modalidad.CalcularPrecio(4);
        Assert.That(precioCuatro, Is.EqualTo(4000m));

        // 5 entradas -> 5000 * 0.85 = 4250
        decimal precioCinco = modalidad.CalcularPrecio(5);
        Assert.That(precioCinco, Is.EqualTo(4250m));

        // 10 entradas -> 10000 * 0.85 = 8500
        decimal precioDiez = modalidad.CalcularPrecio(10);
        Assert.That(precioDiez, Is.EqualTo(8500m));
    }

    [Test]
    public void RealizarCompra_SinCupoDisponible_LanzaExcepcion()
    {
        var evento = _eventoService.Crear("Festival", "Desc", DateTime.Now.AddDays(10), "Estadio");
        var mod = _eventoService.AgregarModalidad(evento.Id, "VIP", 5000m, "Bar", 2);

        // Intentar comprar 3 entradas cuando solo hay 2
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _compraService.RealizarCompra("40123456", evento.Id, mod.Id, 3);
        });

        Assert.That(ex!.Message, Does.Contain("No hay suficiente cupo disponible"));
    }

    [Test]
    public void RealizarCompra_EventoCancelado_LanzaExcepcion()
    {
        var evento = _eventoService.Crear("Concierto", "Desc", DateTime.Now.AddDays(5), "Teatro");
        var mod = _eventoService.AgregarModalidad(evento.Id, "Platea", 2000m, "Ubicación fija", 100);
        _eventoService.Cancelar(evento.Id);

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _compraService.RealizarCompra("40123456", evento.Id, mod.Id, 1);
        });

        Assert.That(ex!.Message, Does.Contain("evento cancelado"));
    }

    [Test]
    public void RealizarCompra_EventoConFechaPasada_LanzaExcepcion()
    {
        // Evento con fecha pasada
        var eventos = _eventoRepo.ObtenerEventos();
        var eventoPasado = new Evento("Evento Ayer", "Desc", DateTime.Now.AddDays(-1), "Teatro");
        var mod = new Modalidad("General", 1000m, "General", 50);
        eventoPasado.AgregarModalidad(mod);
        eventos.Add(eventoPasado);
        _eventoRepo.GuardarEventos(eventos);

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _compraService.RealizarCompra("40123456", eventoPasado.Id, mod.Id, 1);
        });

        Assert.That(ex!.Message, Does.Contain("fecha ya pasó"));
    }

    [Test]
    public void RealizarCompra_GeneraEntradasIndividualesConCodigosUnicosDe6Caracteres()
    {
        var evento = _eventoService.Crear("Recital", "Desc", DateTime.Now.AddDays(20), "Luna Park");
        var mod = _eventoService.AgregarModalidad(evento.Id, "Campo", 1500m, "De pie", 50);

        var compra = _compraService.RealizarCompra("40123456", evento.Id, mod.Id, 5);

        Assert.That(compra.Entradas.Count, Is.EqualTo(5));
        var codigos = new HashSet<string>();

        foreach (var entrada in compra.Entradas)
        {
            Assert.That(entrada.Codigo.Length, Is.EqualTo(6), "El código debe tener exactamente 6 caracteres");
            Assert.That(entrada.Codigo, Does.Match("^[A-Z0-9]{6}$"), "El código debe ser alfanumérico en mayúsculas");
            Assert.That(codigos.Add(entrada.Codigo), Is.True, "Cada entrada debe tener un código único sin repetición");
            Assert.That(entrada.Usada, Is.False, "La entrada nueva no debe estar usada");
        }
    }

    [Test]
    public void ValidacionRoles_OrganizadorYComprador_AplicaRestriccionesCorrectas()
    {
        // Lucía es Organizador
        Assert.DoesNotThrow(() => _usuarioService.ValidarRol("30111222", RolUsuario.Organizador));
        Assert.Throws<UnauthorizedAccessException>(() => _usuarioService.ValidarRol("30111222", RolUsuario.Comprador));

        // Sofía es Comprador
        Assert.DoesNotThrow(() => _usuarioService.ValidarRol("40123456", RolUsuario.Comprador));
        Assert.Throws<UnauthorizedAccessException>(() => _usuarioService.ValidarRol("40123456", RolUsuario.Organizador));

        // DNI inexistente o nulo
        Assert.Throws<UnauthorizedAccessException>(() => _usuarioService.ValidarRol("99999999", RolUsuario.Organizador));
        Assert.Throws<UnauthorizedAccessException>(() => _usuarioService.ValidarRol(null, RolUsuario.Comprador));
    }

    [Test]
    public void CancelarEntrada_CompradorCancelaEntradaNoUsada_RestauraCupoYMarcaCancelada()
    {
        var evento = _eventoService.Crear("Festival Rock", "Desc", DateTime.Now.AddDays(30), "Predio");
        var mod = _eventoService.AgregarModalidad(evento.Id, "General", 2000m, "General", 10);

        var compra = _compraService.RealizarCompra("40123456", evento.Id, mod.Id, 2);
        var entradaACancelar = compra.Entradas[0];

        // Verificar cupo restante antes de cancelar: 10 - 2 = 8
        var eventoAntes = _eventoService.ObtenerPorId(evento.Id)!;
        Assert.That(eventoAntes.ObtenerModalidadPorId(mod.Id)!.CupoDisponible, Is.EqualTo(8));

        // Cancelar una entrada
        var entradaCancelada = _compraService.CancelarEntrada(entradaACancelar.Codigo, "40123456");
        Assert.That(entradaCancelada.Cancelada, Is.True);

        // Verificar que el cupo volvió a 9
        var eventoDespues = _eventoService.ObtenerPorId(evento.Id)!;
        Assert.That(eventoDespues.ObtenerModalidadPorId(mod.Id)!.CupoDisponible, Is.EqualTo(9));
    }

    [Test]
    public void CancelarEntrada_EntradaYaUsada_LanzaExcepcion()
    {
        var evento = _eventoService.Crear("Fiesta", "Desc", DateTime.Now.AddDays(5), "Club");
        var mod = _eventoService.AgregarModalidad(evento.Id, "VIP", 3000m, "VIP", 10);

        var compra = _compraService.RealizarCompra("40123456", evento.Id, mod.Id, 1);
        var entrada = compra.Entradas[0];

        // Marcar entrada como usada en puerta
        entrada.MarcarComoUsada();
        _compraRepo.GuardarCompras(new List<Compra> { compra });

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            _compraService.CancelarEntrada(entrada.Codigo, "40123456");
        });

        Assert.That(ex!.Message, Does.Contain("ya fue utilizada"));
    }

    [Test]
    public void ReporteRecaudacion_CalculaEntradasVendidasYTotalesCorrectamente()
    {
        var evento = _eventoService.Crear("Conferencia Tech", "Desc", DateTime.Now.AddDays(15), "Centro de Convenciones");
        var mod = _eventoService.AgregarModalidad(evento.Id, "Pase Completo", 1000m, "Acceso a todo", 20);

        _compraService.RealizarCompra("40123456", evento.Id, mod.Id, 5); // 5 * 1000 * 0.85 = 4250

        var reporte = _eventoService.ObtenerReporteRecaudacion();
        var repEvento = reporte.Eventos.First(e => e.IdEvento == evento.Id);

        Assert.That(repEvento.EntradasVendidas, Is.EqualTo(5));
        Assert.That(repEvento.RecaudacionTotal, Is.EqualTo(4250m));
        Assert.That(reporte.TotalGeneralRecaudado, Is.EqualTo(4250m));
        Assert.That(reporte.TotalGeneralEntradasVendidas, Is.EqualTo(5));
    }
}
