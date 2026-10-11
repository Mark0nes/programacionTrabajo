namespace GestionEventos.Logica.Excepciones;

/// <summary>DNI ausente o inexistente, rol no permitido o recurso ajeno (HTTP 403).</summary>
public class AccesoDenegadoException : DominioException
{
    public AccesoDenegadoException(string mensaje) : base(mensaje) { }
}
