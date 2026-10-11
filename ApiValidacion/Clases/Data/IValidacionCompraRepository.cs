namespace ValidacionEventos.Data;

using ValidacionEventos.Dominio;

public interface IValidacionCompraRepository
{
    List<Compra> ObtenerCompras();
    void GuardarCompras(List<Compra> compras);
}
