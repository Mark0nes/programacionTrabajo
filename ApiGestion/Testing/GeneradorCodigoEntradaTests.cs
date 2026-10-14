using GestionEventos.Logica;

namespace Testing;

[TestFixture]
public class GeneradorCodigoEntradaTests
{
    [Test]
    public void GenerarCodigoUnico_ConjuntoVacio_DevuelveSeisCaracteresAlfanumericos()
    {
        var existentes = new HashSet<string>();

        string codigo = GeneradorCodigoEntrada.GenerarCodigoUnico(existentes);

        Assert.That(codigo, Does.Match("^[A-Z0-9]{6}$"));
    }

    [Test]
    public void GenerarCodigoUnico_MilCodigosSeguidos_NoSeRepiteNinguno()
    {
        var existentes = new HashSet<string>();
        var generados = new List<string>();

        for (int i = 0; i < 1000; i++)
        {
            generados.Add(GeneradorCodigoEntrada.GenerarCodigoUnico(existentes));
        }

        Assert.That(generados.Distinct().Count(), Is.EqualTo(1000));
    }

    [Test]
    public void GenerarCodigoUnico_ConCodigosYaExistentes_NoDevuelveNingunoDeEllos()
    {
        var existentes = new HashSet<string>();
        for (int i = 0; i < 2000; i++)
        {
            GeneradorCodigoEntrada.GenerarCodigoUnico(existentes);
        }
        var anteriores = new HashSet<string>(existentes);

        string nuevo = GeneradorCodigoEntrada.GenerarCodigoUnico(existentes);

        Assert.That(anteriores.Contains(nuevo), Is.False);
    }

}
