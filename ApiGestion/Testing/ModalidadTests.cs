using GestionEventos.Logica;
using GestionEventos.Logica.Excepciones;

namespace Testing;

[TestFixture]
public class ModalidadTests
{
    private static Modalidad NuevaModalidad(int cupo = 50) => new Modalidad("General", 1000m, "Acceso general", cupo);

    [Test]
    public void CalcularPrecio_UnaEntrada_CobraElPrecioDeLista()
    {
        // Arrange
        var modalidad = NuevaModalidad();

        // Act
        decimal total = modalidad.CalcularPrecio(1);

        // Assert
        Assert.That(total, Is.EqualTo(1000m));
    }

    [Test]
    public void CalcularPrecio_CuatroEntradas_NoAplicaDescuento()
    {
        var modalidad = NuevaModalidad();

        decimal total = modalidad.CalcularPrecio(4);

        Assert.That(total, Is.EqualTo(4000m));
    }

    [Test]
    public void CalcularPrecio_CincoEntradas_AplicaDescuentoDel15PorCiento()
    {
        var modalidad = NuevaModalidad();

        decimal total = modalidad.CalcularPrecio(5);

        Assert.That(total, Is.EqualTo(4250m));
    }

    [Test]
    public void CalcularPrecio_DiezEntradas_AplicaDescuentoDel15PorCiento()
    {
        var modalidad = NuevaModalidad();

        decimal total = modalidad.CalcularPrecio(10);

        Assert.That(total, Is.EqualTo(8500m));
    }

    [Test]
    public void CalcularPrecio_CantidadCero_LanzaReglaNegocioException()
    {
        var modalidad = NuevaModalidad();

        Assert.Throws<ReglaNegocioException>(() => modalidad.CalcularPrecio(0));
    }

    [Test]
    public void RegistrarVenta_CantidadMayorAlCupo_LanzaCupoInsuficienteException()
    {
        var modalidad = NuevaModalidad(cupo: 2);

        Assert.Throws<CupoInsuficienteException>(() => modalidad.RegistrarVenta(3));
    }

    [Test]
    public void RegistrarVenta_ExactamenteElCupoRestante_DejaElCupoEnCero()
    {
        var modalidad = NuevaModalidad(cupo: 3);

        modalidad.RegistrarVenta(3);

        Assert.That(modalidad.CupoDisponible, Is.EqualTo(0));
    }

    [Test]
    public void RegistrarVenta_ModalidadCancelada_LanzaReglaNegocioException()
    {
        var modalidad = NuevaModalidad();
        modalidad.Cancelar();

        Assert.Throws<ReglaNegocioException>(() => modalidad.RegistrarVenta(1));
    }

    [Test]
    public void RestaurarCupo_CuandoYaEstaCompleto_NoSuperaElCupoMaximo()
    {
        var modalidad = NuevaModalidad(cupo: 10);

        modalidad.RestaurarCupo(1);

        Assert.That(modalidad.CupoDisponible, Is.EqualTo(10));
    }

    [Test]
    public void Constructor_PrecioCero_LanzaReglaNegocioException()
    {
        Assert.Throws<ReglaNegocioException>(() => new Modalidad("General", 0m, "Acceso", 10));
    }

    [Test]
    public void Constructor_CupoCero_LanzaReglaNegocioException()
    {
        Assert.Throws<ReglaNegocioException>(() => new Modalidad("General", 1000m, "Acceso", 0));
    }
}
