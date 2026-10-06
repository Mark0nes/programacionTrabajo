namespace GestionEventos.Logica.Excepciones;

public class AccesoDenegadoException : UnauthorizedAccessException
{
    public AccesoDenegadoException(string mensaje) : base(mensaje) { }
}

public class ReglaNegocioException : InvalidOperationException
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}

public class CupoInsuficienteException : ReglaNegocioException
{
    public CupoInsuficienteException(string mensaje) : base(mensaje) { }
}

public class EntidadNoEncontradaException : KeyNotFoundException
{
    public EntidadNoEncontradaException(string mensaje) : base(mensaje) { }
}
