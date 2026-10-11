using GestionEventos.Logica;
using GestionEventos.Logica.Excepciones;

namespace Testing;

[TestFixture]
public class EventoServiceTests : GestionTestBase
{
    [Test]
    public void Crear_ComoOrganizador_GuardaElEventoSinCancelar()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Recital", "Desc", Ahora.AddDays(5), "Luna Park");

        var guardado = EventoSvc.ObtenerPorId(evento.Id);

        Assert.That(guardado.Cancelado, Is.False);
    }

    [Test]
    public void Crear_ComoOrganizador_ConservaNombreLugarYUbicacionAlVolverALeerElArchivo()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Recital", "Desc", Ahora.AddDays(5), "Luna Park", -34.6, -58.4);

        var guardado = EventoSvc.ObtenerPorId(evento.Id);

        Assert.Multiple(() =>
        {
            Assert.That(guardado.Nombre, Is.EqualTo("Recital"));
            Assert.That(guardado.Lugar, Is.EqualTo("Luna Park"));
            Assert.That(guardado.Latitud, Is.EqualTo(-34.6));
            Assert.That(guardado.Longitud, Is.EqualTo(-58.4));
        });
    }

    [Test]
    public void Crear_ComoComprador_LanzaAccesoDenegadoException()
    {
        Assert.Throws<AccesoDenegadoException>(() =>
            EventoSvc.Crear(DniComprador, "Recital", "Desc", Ahora.AddDays(5), "Luna Park"));
    }

    [Test]
    public void Crear_SinDni_LanzaAccesoDenegadoException()
    {
        Assert.Throws<AccesoDenegadoException>(() =>
            EventoSvc.Crear(null, "Recital", "Desc", Ahora.AddDays(5), "Luna Park"));
    }

    [Test]
    public void Crear_ConFechaPasada_LanzaReglaNegocioException()
    {
        Assert.Throws<ReglaNegocioException>(() =>
            EventoSvc.Crear(DniOrganizador, "Recital", "Desc", Ahora.AddDays(-1), "Luna Park"));
    }

    [Test]
    public void Crear_SinNombre_LanzaReglaNegocioException()
    {
        Assert.Throws<ReglaNegocioException>(() =>
            EventoSvc.Crear(DniOrganizador, "  ", "Desc", Ahora.AddDays(5), "Luna Park"));
    }

    [Test]
    public void Modificar_ComoOrganizador_ActualizaLosDatos()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Recital", "Desc", Ahora.AddDays(5), "Luna Park");

        EventoSvc.Modificar(DniOrganizador, evento.Id, "Recital Nuevo", "Otra", Ahora.AddDays(6), "Teatro");

        Assert.That(EventoSvc.ObtenerPorId(evento.Id).Nombre, Is.EqualTo("Recital Nuevo"));
    }

    [Test]
    public void Modificar_ComoComprador_LanzaAccesoDenegadoException()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Recital", "Desc", Ahora.AddDays(5), "Luna Park");

        Assert.Throws<AccesoDenegadoException>(() =>
            EventoSvc.Modificar(DniComprador, evento.Id, "Hack", "Desc", Ahora.AddDays(6), "Teatro"));
    }

    [Test]
    public void Modificar_EventoCancelado_LanzaReglaNegocioException()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Recital", "Desc", Ahora.AddDays(5), "Luna Park");
        EventoSvc.Cancelar(DniOrganizador, evento.Id);

        Assert.Throws<ReglaNegocioException>(() =>
            EventoSvc.Modificar(DniOrganizador, evento.Id, "Nuevo", "Desc", Ahora.AddDays(6), "Teatro"));
    }

    [Test]
    public void Cancelar_ComoOrganizador_DejaElEventoCancelado()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Expo", "Desc", Ahora.AddDays(5), "Predio");

        EventoSvc.Cancelar(DniOrganizador, evento.Id);

        Assert.That(EventoSvc.ObtenerPorId(evento.Id).Cancelado, Is.True);
    }

    [Test]
    public void Cancelar_ComoComprador_LanzaAccesoDenegadoException()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Expo", "Desc", Ahora.AddDays(5), "Predio");

        Assert.Throws<AccesoDenegadoException>(() => EventoSvc.Cancelar(DniComprador, evento.Id));
    }

    [Test]
    public void Cancelar_EventoInexistente_LanzaEntidadNoEncontradaException()
    {
        Assert.Throws<EntidadNoEncontradaException>(() => EventoSvc.Cancelar(DniOrganizador, Guid.NewGuid()));
    }

    [Test]
    public void AgregarModalidad_ComoOrganizador_QuedaEnElEvento()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Festival", "Desc", Ahora.AddDays(5), "Predio");

        EventoSvc.AgregarModalidad(DniOrganizador, evento.Id, "VIP", 5000m, "Barra libre", 20);

        Assert.That(EventoSvc.ObtenerPorId(evento.Id).Modalidades, Has.Count.EqualTo(1));
    }

    [Test]
    public void AgregarModalidad_ConservaPrecioYBeneficiosAlVolverALeerElArchivo()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Festival", "Desc", Ahora.AddDays(5), "Predio");

        var modalidad = EventoSvc.AgregarModalidad(DniOrganizador, evento.Id, "VIP", 5000m, "Barra libre", 20);

        var guardada = ModalidadGuardada(evento.Id, modalidad.Id);
        Assert.Multiple(() =>
        {
            Assert.That(guardada.Nombre, Is.EqualTo("VIP"));
            Assert.That(guardada.Beneficios, Is.EqualTo("Barra libre"));
            Assert.That(guardada.Precio, Is.EqualTo(5000m));
        });
    }

    [Test]
    public void AgregarModalidad_ComoComprador_LanzaAccesoDenegadoException()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Festival", "Desc", Ahora.AddDays(5), "Predio");

        Assert.Throws<AccesoDenegadoException>(() =>
            EventoSvc.AgregarModalidad(DniComprador, evento.Id, "VIP", 5000m, "Barra", 20));
    }

    [Test]
    public void AgregarModalidad_EventoCancelado_LanzaReglaNegocioException()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Festival", "Desc", Ahora.AddDays(5), "Predio");
        EventoSvc.Cancelar(DniOrganizador, evento.Id);

        Assert.Throws<ReglaNegocioException>(() =>
            EventoSvc.AgregarModalidad(DniOrganizador, evento.Id, "VIP", 5000m, "Barra", 20));
    }

    [Test]
    public void CancelarModalidad_ComoOrganizador_QuedaCancelada()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        EventoSvc.CancelarModalidad(DniOrganizador, evento.Id, modalidad.Id);

        Assert.That(ModalidadGuardada(evento.Id, modalidad.Id).Cancelada, Is.True);
    }

    [Test]
    public void ObtenerDisponibles_EventoCancelado_NoApareceEnElCatalogo()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Expo", "Desc", Ahora.AddDays(5), "Predio");
        EventoSvc.Cancelar(DniOrganizador, evento.Id);

        var disponibles = EventoSvc.ObtenerDisponibles();

        Assert.That(disponibles.Any(e => e.Id == evento.Id), Is.False);
    }

    [Test]
    public void ObtenerDisponibles_EventoYaPasado_NoApareceEnElCatalogo()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Expo", "Desc", Ahora.AddDays(5), "Predio");
        AhoraActual = Ahora.AddDays(6);

        var disponibles = EventoSvc.ObtenerDisponibles();

        Assert.That(disponibles.Any(e => e.Id == evento.Id), Is.False);
    }

    [Test]
    public void ObtenerDisponibles_EventoFuturoNoCancelado_ApareceEnElCatalogo()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Expo", "Desc", Ahora.AddDays(5), "Predio");

        var disponibles = EventoSvc.ObtenerDisponibles();

        Assert.That(disponibles.Any(e => e.Id == evento.Id), Is.True);
    }

    [Test]
    public void ObtenerTodos_EventoCancelado_SigueListado()
    {
        var evento = EventoSvc.Crear(DniOrganizador, "Expo", "Desc", Ahora.AddDays(5), "Predio");
        EventoSvc.Cancelar(DniOrganizador, evento.Id);

        var todos = EventoSvc.ObtenerTodos();

        Assert.That(todos.Any(e => e.Id == evento.Id), Is.True);
    }

    [Test]
    public void ObtenerPorId_EventoInexistente_LanzaEntidadNoEncontradaException()
    {
        Assert.Throws<EntidadNoEncontradaException>(() => EventoSvc.ObtenerPorId(Guid.NewGuid()));
    }
}
