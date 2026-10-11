using GestionEventos.Logica;
using Newtonsoft.Json.Linq;

namespace Testing;

/// <summary>
/// La API de Validación lee y reescribe el mismo compras.json. Estos tests fijan el formato que Gestión
/// escribe, para que un cambio de nombre de propiedad no rompa en silencio a la otra API.
/// </summary>
[TestFixture]
public class FormatoCompartidoTests : GestionTestBase
{
    private JObject PrimeraEntradaDelArchivo()
    {
        var compras = JArray.Parse(File.ReadAllText(RutaTemporal("compras.json")));
        return (JObject)compras[0]["Entradas"]![0]!;
    }

    [Test]
    public void GuardarCompras_Entrada_EscribeLasPropiedadesQueValidacionNecesita()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        Comprar(evento, modalidad, 1);

        var entrada = PrimeraEntradaDelArchivo();

        Assert.That(entrada.Properties().Select(p => p.Name),
            Is.SupersetOf(new[] { "Codigo", "IdEvento", "NombreEvento", "NombreModalidad", "Usada", "FechaUso", "Cancelada" }));
    }

    [Test]
    public void GuardarCompras_Entrada_ElCodigoQuedaEnMayusculas()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        Comprar(evento, modalidad, 1);

        string codigo = PrimeraEntradaDelArchivo()["Codigo"]!.ToString();

        Assert.That(codigo, Is.EqualTo(codigo.ToUpperInvariant()));
    }

    [Test]
    public void ObtenerCompras_EntradaUsadaEnElArchivo_SeCargaComoUsada()
    {
        var (evento, modalidad) = CrearEventoConModalidad();
        Comprar(evento, modalidad, 1);
        var compras = CompraRepo.ObtenerCompras();
        compras[0].Entradas[0].MarcarComoUsada(Ahora);
        CompraRepo.GuardarCompras(compras);

        var releidas = CompraRepo.ObtenerCompras();

        Assert.That(releidas[0].Entradas[0].Usada, Is.True);
    }
}
