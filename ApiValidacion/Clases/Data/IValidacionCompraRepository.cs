namespace ValidacionEventos.Data;

using ValidacionEventos.Dominio;

public interface IValidacionCompraRepository
{
    List<Compra> ObtenerCompras();
    void GuardarCompras(List<Compra> compras);
    (Compra? Compra, Entrada? Entrada) BuscarPorCodigo(string codigo);
}
