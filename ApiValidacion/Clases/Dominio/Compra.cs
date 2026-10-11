namespace ValidacionEventos.Dominio;

using Newtonsoft.Json;

/// <summary>
/// Compra tal como la guarda Gestión. Validación solo cambia el estado de sus entradas; el resto de los
/// campos se conserva para que al reescribir compras.json no se pierda nada de lo que escribió Gestión.
/// </summary>
public class Compra
{
    [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
    [JsonProperty] public string DniComprador { get; private set; } = string.Empty;
    [JsonProperty] public DateTime FechaCompra { get; private set; }
    [JsonProperty] public Guid IdEvento { get; private set; }
    [JsonProperty] public string NombreEvento { get; private set; } = string.Empty;
    [JsonProperty] public Guid IdModalidad { get; private set; }
    [JsonProperty] public string NombreModalidad { get; private set; } = string.Empty;
    [JsonProperty] public int Cantidad { get; private set; }
    [JsonProperty] public decimal PrecioUnitario { get; private set; }
    [JsonProperty] public decimal Total { get; private set; }
    [JsonProperty] public List<Entrada> Entradas { get; private set; } = new List<Entrada>();

    public Compra()
    {
    }

    public Compra(string dniComprador, Guid idEvento, string nombreEvento, Guid idModalidad, string nombreModalidad, int cantidad, decimal precioUnitario, DateTime fechaCompra)
    {
        Id = Guid.NewGuid();
        DniComprador = dniComprador;
        FechaCompra = fechaCompra;
        IdEvento = idEvento;
        NombreEvento = nombreEvento;
        IdModalidad = idModalidad;
        NombreModalidad = nombreModalidad;
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
    }

    public void AgregarEntrada(Entrada entrada)
    {
        Entradas.Add(entrada);
        Total += entrada.PrecioUnitario;
    }
}
