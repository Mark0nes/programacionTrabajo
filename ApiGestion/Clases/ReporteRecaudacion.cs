namespace GestionEventos.Logica;
public class ReporteRecaudacion
{
    public decimal TotalGeneralRecaudado { get; private set; }
    public int TotalGeneralEntradasVendidas { get; private set; }
    public List<ReporteEvento> Eventos { get; private set; } = new List<ReporteEvento>();

    public void ActualizarTotales()
    {
        TotalGeneralRecaudado = Eventos.Sum(e => e.RecaudacionTotal);
        TotalGeneralEntradasVendidas = Eventos.Sum(e => e.EntradasVendidas);
    }
}