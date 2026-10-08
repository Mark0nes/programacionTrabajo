namespace ValidacionEventos.Logica;

public class ResultadoValidacion
{
    public bool Exitoso { get; private set; }
    public EstadoValidacion Estado { get; private set; }
    public string Mensaje { get; private set; } = string.Empty;
    public string? Codigo { get; private set; }
    public string? NombreEvento { get; private set; }
    public string? NombreModalidad { get; private set; }
    public DateTime? FechaUso { get; private set; }

    public ResultadoValidacion(bool exitoso, EstadoValidacion estado, string mensaje, string? codigo = null, string? nombreEvento = null, string? nombreModalidad = null, DateTime? fechaUso = null)
    {
        Exitoso = exitoso;
        Estado = estado;
        Mensaje = mensaje;
        Codigo = codigo;
        NombreEvento = nombreEvento;
        NombreModalidad = nombreModalidad;
        FechaUso = fechaUso;
    }
}
