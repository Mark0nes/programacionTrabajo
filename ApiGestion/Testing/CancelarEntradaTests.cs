using GestionEventos.Logica;
using GestionEventos.Logica.Excepciones;

namespace Testing;

[TestFixture]
public class CancelarEntradaTests : GestionTestBase
{
    private Entrada EntradaGuardada(string codigo)
    {
        return CompraRepo.ObtenerCompras().SelectMany(c => c.Entradas).Single(e => e.Codigo == codigo);
    }

    private void MarcarEntradaComoUsada(string codigo)
    {
        var compras = CompraRepo.ObtenerCompras();
        compras.SelectMany(c => c.Entradas).Single(e => e.Codigo == codigo).MarcarComoUsada(Ahora);
        CompraRepo.GuardarCompras(compras);
    }

    [Test]
    public void CancelarEntrada_EntradaPropia_QuedaCancelada()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 2);
        string codigo = compra.Entradas[0].Codigo;

        CompraSvc.CancelarEntrada(codigo, DniComprador);

        Assert.That(EntradaGuardada(codigo).Cancelada, Is.True);
    }

    [Test]
    public void CancelarEntrada_EntradaPropia_DevuelveElLugarAlCupo()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 10);
        var compra = Comprar(evento, modalidad, 2);

        CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniComprador);

        Assert.That(ModalidadGuardada(evento.Id, modalidad.Id).CupoDisponible, Is.EqualTo(9));
    }

    [Test]
    public void CancelarEntrada_UnaDeLasEntradas_LasDemasSiguenVigentes()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 3);

        CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniComprador);

        Assert.That(EntradaGuardada(compra.Entradas[1].Codigo).Cancelada, Is.False);
    }

    [Test]
    public void CancelarEntrada_CompraConDescuento_ElTotalBajaEnLoQueEsaEntradaPago()
    {
        var (evento, modalidad) = CrearEventoConModalidad(precio: 1000m);
        var compra = Comprar(evento, modalidad, 5);

        CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniComprador);

        var actualizada = CompraSvc.ObtenerPorId(compra.Id, DniComprador);
        Assert.That(actualizada.Total, Is.EqualTo(3400m));
    }

    [Test]
    public void CancelarEntrada_CodigoEnMinusculas_LaEncuentra()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);

        CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo.ToLowerInvariant(), DniComprador);

        Assert.That(EntradaGuardada(compra.Entradas[0].Codigo).Cancelada, Is.True);
    }

    [Test]
    public void CancelarEntrada_EntradaYaUsadaEnLaPuerta_LanzaReglaNegocioException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);
        string codigo = compra.Entradas[0].Codigo;
        MarcarEntradaComoUsada(codigo);

        var ex = Assert.Throws<ReglaNegocioException>(() => CompraSvc.CancelarEntrada(codigo, DniComprador));

        Assert.That(ex!.Message, Does.Contain("utilizada"));
    }

    [Test]
    public void CancelarEntrada_EntradaYaUsada_NoDevuelveElCupo()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 10);
        var compra = Comprar(evento, modalidad, 1);
        string codigo = compra.Entradas[0].Codigo;
        MarcarEntradaComoUsada(codigo);

        Assert.Throws<ReglaNegocioException>(() => CompraSvc.CancelarEntrada(codigo, DniComprador));

        Assert.That(ModalidadGuardada(evento.Id, modalidad.Id).CupoDisponible, Is.EqualTo(9));
    }

    [Test]
    public void CancelarEntrada_EntradaYaCancelada_LanzaReglaNegocioException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);
        string codigo = compra.Entradas[0].Codigo;
        CompraSvc.CancelarEntrada(codigo, DniComprador);

        Assert.Throws<ReglaNegocioException>(() => CompraSvc.CancelarEntrada(codigo, DniComprador));
    }

    [Test]
    public void CancelarEntrada_EntradaDeOtroComprador_LanzaAccesoDenegadoException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1, DniComprador);

        Assert.Throws<AccesoDenegadoException>(() =>
            CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniOtroComprador));
    }

    [Test]
    public void CancelarEntrada_EntradaDeOtroComprador_NoLaCancela()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1, DniComprador);
        string codigo = compra.Entradas[0].Codigo;

        Assert.Throws<AccesoDenegadoException>(() => CompraSvc.CancelarEntrada(codigo, DniOtroComprador));

        Assert.That(EntradaGuardada(codigo).Cancelada, Is.False);
    }

    [Test]
    public void CancelarEntrada_ComoOrganizador_LanzaAccesoDenegadoException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);

        Assert.Throws<AccesoDenegadoException>(() =>
            CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniOrganizador));
    }

    [Test]
    public void CancelarEntrada_CodigoInexistente_LanzaEntidadNoEncontradaException()
    {
        Assert.Throws<EntidadNoEncontradaException>(() => CompraSvc.CancelarEntrada("ZZZZZZ", DniComprador));
    }

    [Test]
    public void CancelarEntrada_CodigoVacio_LanzaReglaNegocioException()
    {
        Assert.Throws<ReglaNegocioException>(() => CompraSvc.CancelarEntrada("  ", DniComprador));
    }

    [Test]
    public void CancelarEntrada_EventoYaComenzado_LanzaReglaNegocioException()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        var compra = Comprar(evento, modalidad, 1);
        AhoraActual = Ahora.AddDays(11);

        Assert.Throws<ReglaNegocioException>(() =>
            CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniComprador));
    }

    [Test]
    public void CancelarEntrada_LiberaElLugar_OtroCompradorPuedeComprarla()
    {
        var (evento, modalidad) = CrearEventoConModalidad(cupo: 1);
        var compra = Comprar(evento, modalidad, 1, DniComprador);
        CompraSvc.CancelarEntrada(compra.Entradas[0].Codigo, DniComprador);

        var nueva = Comprar(evento, modalidad, 1, DniOtroComprador);

        Assert.That(nueva.Entradas, Has.Count.EqualTo(1));
    }
}
