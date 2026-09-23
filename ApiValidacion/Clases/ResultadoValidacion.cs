namespace ValidacionEventos.Logica;

public class ResultadoValidacion
{
    public bool Exitoso { get; set; }
    public EstadoValidacion Estado { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public string? Codigo { get; set; }
    public string? NombreEvento { get; set; }
    public string? NombreModalidad { get; set; }
    public DateTime? FechaUso { get; set; }
}
