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
    public void RealizarCompra_CompraValida_DescuentaElCupoDeLaModalidad()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 10);

        Comprar(evento, modalidad, 4);

        Assert.That(ModalidadGuardada(evento.Id, modalidad.Id).CupoDisponible, Is.EqualTo(6));
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
    public void RealizarCompra_EventoInexistente_LanzaEntidadNoEncontradaException()
    {
        Assert.Throws<EntidadNoEncontradaException>(() =>
            CompraSvc.RealizarCompra(DniComprador, Guid.NewGuid(), Guid.NewGuid(), 1));
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

    // ---------- Entradas individuales y códigos ----------

    [Test]
    public void RealizarCompra_VariasEntradas_GeneraUnaEntradaPorCadaUna()
    {
        var (evento, modalidad) = CrearEventoConModalidad();

        var compra = Comprar(evento, modalidad, 3);

        Assert.That(compra.Entradas, Has.Count.EqualTo(3));
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

    // ---------- Persistencia ----------

    // ---------- Consultas ----------

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
    public void ObtenerPorId_SinDni_DevuelveLaCompra()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);

        var leida = CompraSvc.ObtenerPorId(compra.Id, null);

        Assert.That(leida.Id, Is.EqualTo(compra.Id));
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

}
