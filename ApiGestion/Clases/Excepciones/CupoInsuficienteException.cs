namespace GestionEventos.Logica.Excepciones;

/// <summary>No queda cupo suficiente en la modalidad (HTTP 400).</summary>
public class CupoInsuficienteException : ReglaNegocioException
{
    public CupoInsuficienteException(string mensaje) : base(mensaje) { }
}
