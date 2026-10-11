namespace GestionEventos.Logica.Excepciones;

/// <summary>
/// Base de las excepciones propias del dominio. No hereda de ninguna excepción
/// "con significado" del framework (por ejemplo UnauthorizedAccessException, que
/// es para permisos de archivos): el Controller las traduce a 400/403/404.
/// </summary>
public abstract class DominioException : Exception
{
    protected DominioException(string mensaje) : base(mensaje) { }
}
