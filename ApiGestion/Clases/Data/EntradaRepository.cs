namespace GestionEventos.Data;

using GestionEventos.Logica;

public class EntradaRepository
{
    private readonly CompraRepository _compraRepository;

    public EntradaRepository(CompraRepository? compraRepository = null)
    {
        _compraRepository = compraRepository ?? new CompraRepository();
    }

    public List<Entrada> ObtenerEntradas()
    {
        var compras = _compraRepository.ObtenerCompras();
        return compras.SelectMany(c => c.Entradas).ToList();
    }

    public Entrada? ObtenerPorCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        string codLimpio = codigo.Trim().ToUpperInvariant();
        return ObtenerEntradas().FirstOrDefault(e => e.Codigo.Equals(codLimpio, StringComparison.OrdinalIgnoreCase));
    }

    public void ActualizarEntrada(Entrada entradaModificada)
    {
        var compras = _compraRepository.ObtenerCompras();
        bool encontrada = false;

        foreach (var compra in compras)
        {
            var idx = compra.Entradas.FindIndex(e => e.Codigo.Equals(entradaModificada.Codigo, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
            {
                compra.Entradas[idx] = entradaModificada;
                encontrada = true;
                break;
            }
        }

        if (encontrada)
        {
            _compraRepository.GuardarCompras(compras);
        }
    }
}