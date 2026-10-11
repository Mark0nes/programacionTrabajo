namespace GestionEventos.Logica.Excepciones;

/// <summary>Se violó una regla del negocio o un dato es inválido (HTTP 400).</summary>
public class ReglaNegocioException : DominioException
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}
