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
    public void ObtenerReporteRecaudacion_VariosEventos_LosTotalesGeneralesSumanTodos()
    {
        var (evento1, modalidad1) = CrearEventoConModalidad(precio: 1000m);
        var (evento2, modalidad2) = CrearEventoConModalidad(precio: 2000m);
        Comprar(evento1, modalidad1, 1);
        Comprar(evento2, modalidad2, 1);

        var reporte = EventoSvc.ObtenerReporteRecaudacion(DniOrganizador);

        Assert.That(reporte.TotalGeneralRecaudado, Is.EqualTo(3000m));
    }

}
