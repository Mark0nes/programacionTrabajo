using GestionEventos.Logica;
using GestionEventos.Logica.Excepciones;

namespace Testing;

[TestFixture]
public class ReporteRecaudacionTests : GestionTestBase
{
    private ReporteEvento ReporteDe(Evento evento)
    {
        return EventoSvc.ObtenerReporteRecaudacion(DniOrganizador).Eventos.Single(e => e.IdEvento == evento.Id);
    }

    [Test]
    public void ObtenerReporteRecaudacion_ComoComprador_LanzaAccesoDenegadoException()
    {
        Assert.Throws<AccesoDenegadoException>(() => EventoSvc.ObtenerReporteRecaudacion(DniComprador));
    }

    [Test]
    public void ObtenerReporteRecaudacion_SinDni_LanzaAccesoDenegadoException()
    {
        Assert.Throws<AccesoDenegadoException>(() => EventoSvc.ObtenerReporteRecaudacion(null));
    }

    [Test]
    public void ObtenerReporteRecaudacion_EventoSinCompras_InformaCeroEntradas()
    {
        var (evento, _) = CrearEventoConModalidad();

        var reporte = ReporteDe(evento);

        Assert.That(reporte.EntradasVendidas, Is.EqualTo(0));
    }

    [Test]
    public void ObtenerReporteRecaudacion_CompraDeCinco_InformaCincoEntradas()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);
        Comprar(evento, modalidad, 5);

        var reporte = ReporteDe(evento);

        Assert.That(reporte.EntradasVendidas, Is.EqualTo(5));
    }

    [Test]
    public void ObtenerReporteRecaudacion_CompraDeCinco_InformaElDineroConDescuento()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);
        Comprar(evento, modalidad, 5);

        var reporte = ReporteDe(evento);

        Assert.That(reporte.RecaudacionTotal, Is.EqualTo(4250m));
    }

    [Test]
    public void ObtenerReporteRecaudacion_DespuesDeCancelarUnaEntrada_BajaLaCantidadVendida()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);
        var compra = Comprar(evento, modalidad, 5);
        CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniComprador);

        var reporte = ReporteDe(evento);

        Assert.That(reporte.EntradasVendidas, Is.EqualTo(4));
    }

    [Test]
    public void ObtenerReporteRecaudacion_DespuesDeCancelarUnaEntrada_BajaElDineroEnLoQueEsaEntradaPago()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);
        var compra = Comprar(evento, modalidad, 5);
        CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniComprador);

        var reporte = ReporteDe(evento);

        Assert.That(reporte.RecaudacionTotal, Is.EqualTo(3400m));
    }

    [Test]
    public void ObtenerReporteRecaudacion_CancelandoTodasLasEntradas_QuedaEnCero()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);
        var compra = Comprar(evento, modalidad, 2);
        foreach (var entrada in compra.Entradas)
        {
            CompraSvc.CancelarEntrada(entrada.Codigo, DniComprador);
        }

        var reporte = ReporteDe(evento);

        Assert.That(reporte.RecaudacionTotal, Is.EqualTo(0m));
    }

    [Test]
    public void ObtenerReporteRecaudacion_DosCompradores_ListaACadaUno()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        Comprar(evento, modalidad, 1, DniComprador);
        Comprar(evento, modalidad, 2, DniOtroComprador);

        var reporte = ReporteDe(evento);

        Assert.That(reporte.Compradores, Has.Count.EqualTo(2));
    }

    [Test]
    public void ObtenerReporteRecaudacion_UnCompradorConDosCompras_SeAcumulaEnUnaSolaFila()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        Comprar(evento, modalidad, 1, DniComprador);
        Comprar(evento, modalidad, 2, DniComprador);

        var reporte = ReporteDe(evento);

        Assert.That(reporte.Compradores.Single().CantidadEntradas, Is.EqualTo(3));
    }

    [Test]
    public void ObtenerReporteRecaudacion_DetalleDeModalidad_InformaSoloLasVendidasDeEsaModalidad()
    {
        var (evento, general) = CrearEventoConModalidad(precio: 1000m);
        var vip = EventoSvc.AgregarModalidad(DniOrganizador, evento.Id, "VIP", 5000m, "Barra", 10);
        Comprar(evento, general, 2);
        Comprar(evento, vip, 1);

        var reporte = ReporteDe(evento);

        Assert.That(reporte.Modalidades.Single(m => m.IdModalidad == vip.Id).Recaudacion, Is.EqualTo(5000m));
    }

    [Test]
    public void ObtenerReporteRecaudacion_VariosEventos_LosTotalesGeneralesSumanTodos()
    {
        var (evento1, modalidad1) = CrearEventoConModalidad(precio: 1000m);
        var (evento2, modalidad2) = CrearEventoConModalidad(precio: 2000m);
        Comprar(evento1, modalidad1, 1);
        Comprar(evento2, modalidad2, 1);

        var reporte = EventoSvc.ObtenerReporteRecaudacion(DniOrganizador);

        Assert.That(reporte.TotalGeneralRecaudado, Is.EqualTo(3000m));
    }

    [Test]
    public void ObtenerReporteRecaudacion_VariosEventos_LaCantidadGeneralSumaTodos()
    {
        var (evento1, modalidad1) = CrearEventoConModalidad(precio: 1000m);
        var (evento2, modalidad2) = CrearEventoConModalidad(precio: 2000m);
        Comprar(evento1, modalidad1, 2);
        Comprar(evento2, modalidad2, 3);

        var reporte = EventoSvc.ObtenerReporteRecaudacion(DniOrganizador);

        Assert.That(reporte.TotalGeneralEntradasVendidas, Is.EqualTo(5));
    }
}
