using GestionEventos.Data;
using GestionEventos.Logica;
using GestionEventos.Logica.Excepciones;

namespace Testing;

[TestFixture]
public class UsuarioServiceTests : GestionTestBase
{
    [Test]
    public void ValidarRol_CompradorPideRolOrganizador_LanzaAccesoDenegadoException()
    {
        Assert.Throws<AccesoDenegadoException>(() => UsuarioSvc.ValidarRol(DniComprador, RolUsuario.Organizador));
    }

    [Test]
    public void ValidarRol_OrganizadorPideRolComprador_LanzaAccesoDenegadoException()
    {
        Assert.Throws<AccesoDenegadoException>(() => UsuarioSvc.ValidarRol(DniOrganizador, RolUsuario.Comprador));
    }

    [Test]
    public void ValidarRol_DniInexistente_LanzaAccesoDenegadoException()
    {
        Assert.Throws<AccesoDenegadoException>(() => UsuarioSvc.ValidarRol("99999999", RolUsuario.Organizador));
    }

    [Test]
    public void ObtenerUsuarios_ArchivoConDniNumerico_LoCargaComoTexto()
    {
        // Arrange: formato exacto del anexo del enunciado (dni como número)
        string ruta = RutaTemporal("usuarios_anexo.json");
        File.WriteAllText(ruta, "[ { \"dni\": 30111222, \"nombre\": \"Lucía Fernández\", \"username\": \"lfernandez\", \"rol\": \"Organizador\" } ]");
        var repositorio = new PersonaRepository(ruta);

        // Act
        var usuarios = repositorio.ObtenerUsuarios();

        // Assert
        Assert.That(usuarios[0].Dni, Is.EqualTo("30111222"));
    }

    [Test]
    public void ObtenerUsuarios_ArchivoConRolTexto_LoCargaComoEnum()
    {
        string ruta = RutaTemporal("usuarios_anexo.json");
        File.WriteAllText(ruta, "[ { \"dni\": 29888777, \"nombre\": \"Martín Aguirre\", \"username\": \"maguirre\", \"rol\": \"Organizador\" } ]");
        var repositorio = new PersonaRepository(ruta);

        var usuarios = repositorio.ObtenerUsuarios();

        Assert.That(usuarios[0].Rol, Is.EqualTo(RolUsuario.Organizador));
    }

}
