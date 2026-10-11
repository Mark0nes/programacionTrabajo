namespace GestionEventos.Logica.Excepciones;

/// <summary>El recurso pedido no existe (HTTP 404).</summary>
public class EntidadNoEncontradaException : DominioException
{
    public EntidadNoEncontradaException(string mensaje) : base(mensaje) { }
}
