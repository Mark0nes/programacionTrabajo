using GestionEventos.Logica;
using GestionEventos.Logica.Excepciones;

namespace Testing;

[TestFixture]
public class CompraServiceTests : GestionTestBase
{
    // ---------- Quién puede comprar ----------

    [Test]
    public void RealizarCompra_CompradorValido_QuedaAsociadaASuDni()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        var compra = Comprar(evento, modalidad, 1);

        Assert.That(compra.DniComprador, Is.EqualTo(DniComprador));
    }

    [Test]
    public void RealizarCompra_DniInexistente_LanzaAccesoDenegadoException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        Assert.Throws<AccesoDenegadoException>(() => Comprar(evento, modalidad, 1, "99999999"));
    }

    [Test]
    public void RealizarCompra_OrganizadorIntentaComprar_LanzaAccesoDenegadoException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        Assert.Throws<AccesoDenegadoException>(() => Comprar(evento, modalidad, 1, DniOrganizador));
    }

    [Test]
    public void RealizarCompra_SinDni_LanzaAccesoDenegadoException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        Assert.Throws<AccesoDenegadoException>(() =>
            CompraSvc.RealizarCompra(null, evento.Id, modalidad.Id, 1));
    }

    // ---------- Cupo ----------

    [Test]
    public void RealizarCompra_SinCupoSuficiente_LanzaCupoInsuficienteException()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 2);

        Assert.Throws<CupoInsuficienteException>(() => Comprar(evento, modalidad, 3));
    }

    [Test]
    public void RealizarCompra_CupoAgotado_RechazaLaSiguienteCompra()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 2);
        Comprar(evento, modalidad, 2);

        Assert.Throws<CupoInsuficienteException>(() => Comprar(evento, modalidad, 1, DniOtroComprador));
    }

    [Test]
    public void RealizarCompra_ExactamenteElCupoRestante_SeAcepta()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 3);

        var compra = Comprar(evento, modalidad, 3);

        Assert.That(compra.Entradas, Has.Count.EqualTo(3));
    }

    [Test]
    public void RealizarCompra_CompraValida_DescuentaElCupoDeLaModalidad()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 10);

        Comprar(evento, modalidad, 4);

        Assert.That(ModalidadGuardada(evento.Id, modalidad.Id).CupoDisponible, Is.EqualTo(6));
    }

    [Test]
    public void RealizarCompra_DeUnaModalidad_NoConsumeElCupoDeOtra()
    {
        var (evento, general) = CrearEventoConModalidad(cupo: 10);
        var vip = EventoSvc.AgregarModalidad(DniOrganizador, evento.Id, "VIP", 5000m, "Barra libre", 5);

        Comprar(evento, vip, 2);

        Assert.That(ModalidadGuardada(evento.Id, general.Id).CupoDisponible, Is.EqualTo(10));
    }

    // ---------- Evento y modalidad ----------

    [Test]
    public void RealizarCompra_EventoCancelado_LanzaReglaNegocioException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        EventoSvc.Cancelar(DniOrganizador, evento.Id);

        var ex = Assert.Throws<ReglaNegocioException>(() => Comprar(evento, modalidad, 1));

        Assert.That(ex!.Message, Does.Contain("cancelado"));
    }

    [Test]
    public void RealizarCompra_EventoConFechaPasada_LanzaReglaNegocioException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        AhoraActual = Ahora.AddDays(11);

        var ex = Assert.Throws<ReglaNegocioException>(() => Comprar(evento, modalidad, 1));

        Assert.That(ex!.Message, Does.Contain("ya pasó"));
    }

    [Test]
    public void RealizarCompra_ModalidadCancelada_LanzaReglaNegocioException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        EventoSvc.CancelarModalidad(DniOrganizador, evento.Id, modalidad.Id);

        Assert.Throws<ReglaNegocioException>(() => Comprar(evento, modalidad, 1));
    }

    [Test]
    public void RealizarCompra_EventoInexistente_LanzaEntidadNoEncontradaException()
    {
        Assert.Throws<EntidadNoEncontradaException>(() =>
            CompraSvc.RealizarCompra(DniComprador, Guid.NewGuid(), Guid.NewGuid(), 1));
    }

    [Test]
    public void RealizarCompra_ModalidadDeOtroEvento_LanzaEntidadNoEncontradaException()
    {
        var (evento, _) = CrearEventoConModalidad();
        var (_, modalidadAjena) = CrearEventoConModalidad();

        Assert.Throws<EntidadNoEncontradaException>(() =>
            CompraSvc.RealizarCompra(DniComprador, evento.Id, modalidadAjena.Id, 1));
    }

    [Test]
    public void RealizarCompra_CantidadCero_LanzaReglaNegocioException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        Assert.Throws<ReglaNegocioException>(() => Comprar(evento, modalidad, 0));
    }

    // ---------- Precio y descuento ----------

    [Test]
    public void RealizarCompra_ExactamenteCuatroEntradas_NoAplicaDescuento()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);

        var compra = Comprar(evento, modalidad, 4);

        Assert.That(compra.Total, Is.EqualTo(4000m));
    }

    [Test]
    public void RealizarCompra_ExactamenteCincoEntradas_AplicaDescuento()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);

        var compra = Comprar(evento, modalidad, 5);

        Assert.That(compra.Total, Is.EqualTo(4250m));
    }

    [Test]
    public void RealizarCompra_CincoEntradas_CadaEntradaGuardaElPrecioConDescuento()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);

        var compra = Comprar(evento, modalidad, 5);

        Assert.That(compra.Entradas.All(e => e.PrecioUnitario == 850m), Is.True);
    }

    [Test]
    public void RealizarCompra_PrecioConRedondeo_LaSumaDeLasEntradasEsElTotalCobrado()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000.01m);

        var compra = Comprar(evento, modalidad, 5);

        Assert.That(compra.Entradas.Sum(e => e.PrecioUnitario), Is.EqualTo(compra.Total));
    }

    // ---------- Entradas individuales y códigos ----------

    [Test]
    public void RealizarCompra_VariasEntradas_GeneraUnaEntradaPorCadaUna()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        var compra = Comprar(evento, modalidad, 3);

        Assert.That(compra.Entradas, Has.Count.EqualTo(3));
    }

    [Test]
    public void RealizarCompra_CompraNueva_EntradasNacenSinUsarNiCancelar()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        var compra = Comprar(evento, modalidad, 3);

        Assert.That(compra.Entradas.Any(e => e.Usada || e.Cancelada), Is.False);
    }

    [Test]
    public void RealizarCompra_VariasEntradas_CadaCodigoTieneSeisCaracteresAlfanumericos()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        var compra = Comprar(evento, modalidad, 5);

        Assert.That(compra.Entradas.All(e => System.Text.RegularExpressions.Regex.IsMatch(e.Codigo, "^[A-Z0-9]{6}$")), Is.True);
    }

    [Test]
    public void RealizarCompra_VariasEntradas_NingunCodigoSeRepiteDentroDeLaCompra()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        var compra = Comprar(evento, modalidad, 20);

        Assert.That(compra.Entradas.Select(e => e.Codigo).Distinct().Count(), Is.EqualTo(20));
    }

    [Test]
    public void RealizarCompra_ComprasDistintas_NingunCodigoSeRepiteEntreCompras()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 100);

        var compra1 = Comprar(evento, modalidad, 20);
        var compra2 = Comprar(evento, modalidad, 20, DniOtroComprador);

        var todos = compra1.Entradas.Concat(compra2.Entradas).Select(e => e.Codigo).Distinct().Count();
        Assert.That(todos, Is.EqualTo(40));
    }

    [Test]
    public void RealizarCompra_CadaEntrada_GuardaElEventoAlQuePertenece()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        var compra = Comprar(evento, modalidad, 2);

        Assert.That(compra.Entradas.All(e => e.IdEvento == evento.Id), Is.True);
    }

    // ---------- Persistencia ----------

    [Test]
    public void RealizarCompra_UnaVezGuardada_SePuedeConsultarYConservaSusEntradas()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 3);

        var leida = CompraSvc.ObtenerPorId(compra.Id, DniComprador);

        Assert.That(leida.Entradas.Select(e => e.Codigo), Is.EquivalentTo(compra.Entradas.Select(e => e.Codigo)));
    }

    // ---------- Consultas ----------

    [Test]
    public void ObtenerPorId_ElCompradorDeLaCompra_LaVe()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);

        var leida = CompraSvc.ObtenerPorId(compra.Id, DniComprador);

        Assert.That(leida.Id, Is.EqualTo(compra.Id));
    }

    [Test]
    public void ObtenerPorId_OtroComprador_LanzaAccesoDenegadoException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);

        Assert.Throws<AccesoDenegadoException>(() => CompraSvc.ObtenerPorId(compra.Id, DniOtroComprador));
    }

    [Test]
    public void ObtenerPorId_Organizador_PuedeVerCualquierCompra()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);

        var leida = CompraSvc.ObtenerPorId(compra.Id, DniOrganizador);

        Assert.That(leida.Id, Is.EqualTo(compra.Id));
    }

    [Test]
    public void ObtenerPorId_SinDni_LanzaAccesoDenegadoException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);

        Assert.Throws<AccesoDenegadoException>(() => CompraSvc.ObtenerPorId(compra.Id, null));
    }

    [Test]
    public void ObtenerPorId_CompraInexistente_LanzaEntidadNoEncontradaException()
    {
        Assert.Throws<EntidadNoEncontradaException>(() => CompraSvc.ObtenerPorId(Guid.NewGuid(), DniComprador));
    }

    [Test]
    public void ObtenerComprasVisibles_Comprador_SoloVeLasSuyas()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        Comprar(evento, modalidad, 1, DniComprador);
        Comprar(evento, modalidad, 1, DniOtroComprador);

        var compras = CompraSvc.ObtenerComprasVisibles(DniComprador);

        Assert.That(compras.All(c => c.DniComprador == DniComprador), Is.True);
    }

    [Test]
    public void ObtenerComprasVisibles_Organizador_VeTodas()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        Comprar(evento, modalidad, 1, DniComprador);
        Comprar(evento, modalidad, 1, DniOtroComprador);

        var compras = CompraSvc.ObtenerComprasVisibles(DniOrganizador);

        Assert.That(compras, Has.Count.EqualTo(2));
    }

    [Test]
    public void ObtenerComprasVisibles_SinDni_LanzaAccesoDenegadoException()
    {
        Assert.Throws<AccesoDenegadoException>(() => CompraSvc.ObtenerComprasVisibles(null));
    }
}
