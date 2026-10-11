namespace GestionEventos.Logica;

public class ReporteRecaudacion
{
    public List<ReporteEvento> Eventos { get; private set; } = new List<ReporteEvento>();

    public decimal TotalGeneralRecaudado => Eventos.Sum(e => e.RecaudacionTotal);

    public int TotalGeneralEntradasVendidas => Eventos.Sum(e => e.EntradasVendidas);

    public void AgregarEvento(ReporteEvento evento)
    {
        Eventos.Add(evento);
    }
}
