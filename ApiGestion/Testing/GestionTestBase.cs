using GestionEventos.Data;
using GestionEventos.Logica;

namespace Testing;

/// <summary>
/// Escenario común de los tests de Gestión: archivos JSON temporales (uno distinto por test, borrados al
/// terminar) y un reloj fijo, para que ningún test dependa de la hora real ni de los datos de /data.
/// </summary>
public abstract class GestionTestBase
{
    protected static readonly DateTime Ahora = new DateTime(2026, 10, 20, 12, 0, 0);

    protected const string DniOrganizador = "30111222";
    protected const string DniComprador = "40123456";
    protected const string DniOtroComprador = "38456789";

    private string _carpeta = string.Empty;

    protected DateTime AhoraActual { get; set; }
    protected EventoRepository EventoRepo { get; private set; } = null!;
    protected CompraRepository CompraRepo { get; private set; } = null!;
    protected PersonaRepository PersonaRepo { get; private set; } = null!;
    protected UsuarioService UsuarioSvc { get; private set; } = null!;
    protected EventoService EventoSvc { get; private set; } = null!;
    protected CompraService CompraSvc { get; private set; } = null!;

    [SetUp]
    public void ArmarEscenario()
    {
        AhoraActual = Ahora;

        _carpeta = Path.Combine(Path.GetTempPath(), "TestGestion_" + Guid.NewGuid());
        Directory.CreateDirectory(_carpeta);

        PersonaRepo = new PersonaRepository(Path.Combine(_carpeta, "usuarios.json"));
        EventoRepo = new EventoRepository(Path.Combine(_carpeta, "eventos.json"));
        CompraRepo = new CompraRepository(Path.Combine(_carpeta, "compras.json"));

        PersonaRepo.GuardarUsuarios(new List<Usuario>
        {
            new Usuario(DniOrganizador, "Lucía Fernández", "lfernandez", RolUsuario.Organizador),
            new Usuario(DniComprador, "Sofía Gómez", "sgomez", RolUsuario.Comprador),
            new Usuario(DniOtroComprador, "Nicolás Pereyra", "npereyra", RolUsuario.Comprador)
        });

        Func<DateTime> reloj = () => AhoraActual;
        UsuarioSvc = new UsuarioService(PersonaRepo);
        EventoSvc = new EventoService(EventoRepo, CompraRepo, UsuarioSvc, reloj);
        CompraSvc = new CompraService(CompraRepo, EventoRepo, UsuarioSvc, reloj);
    }

    [TearDown]
    public void LimpiarEscenario()
    {
        if (Directory.Exists(_carpeta))
        {
            Directory.Delete(_carpeta, true);
        }
    }

    protected string RutaTemporal(string archivo) => Path.Combine(_carpeta, archivo);

    /// <summary>Crea (como organizador) un evento dentro de 10 días con una modalidad.</summary>
    protected (Evento Evento, Modalidad Modalidad) CrearEventoConModalidad(decimal precio = 1000m, int cupo = 50)
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Festival", "Descripción", Ahora.AddDays(10), "Estadio");
        var modalidad = EventoSvc.AgregarModalidad(DniOrganizador, evento.Id, "General", precio, "Acceso general", cupo);
        return (evento, modalidad);
    }

    protected Compra Comprar(Evento evento, Modalidad modalidad, int cantidad, string dni = DniComprador)
    {
        return CompraSvc.RealizarCompra(dni, evento.Id, modalidad.Id, cantidad);
    }

    protected Modalidad ModalidadGuardada(Guid idEvento, Guid idModalidad)
    {
        return EventoSvc.ObtenerPorId(idEvento).ObtenerModalidadPorId(idModalidad)!;
    }
}
