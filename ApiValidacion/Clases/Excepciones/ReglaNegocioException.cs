namespace ValidacionEventos.Logica.Excepciones;

/// <summary>Se intentó una operación que el dominio no permite (por ejemplo, usar dos veces una entrada).</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}
