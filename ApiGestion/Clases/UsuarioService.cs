using GestionEventos.Data;
using GestionEventos.Logica.Excepciones;

namespace GestionEventos.Logica;

public class UsuarioService
{
    private readonly PersonaRepository _personaRepository;

    public UsuarioService(PersonaRepository personaRepository)
    {
        _personaRepository = personaRepository ?? throw new ArgumentNullException(nameof(personaRepository));
    }

    public List<Usuario> ObtenerTodos()
    {
        return _personaRepository.ObtenerUsuarios();
    }

    public Usuario? ObtenerPorDni(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return null;
        string dniLimpio = dni.Trim();
        return _personaRepository.ObtenerUsuarios().FirstOrDefault(u => u.Dni == dniLimpio);
    }

    /// <summary>Exige que haya un DNI y que corresponda a un usuario cargado, sin mirar el rol.</summary>
    public Usuario ValidarUsuario(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni))
        {
            throw new AccesoDenegadoException("Se requiere especificar el DNI del usuario que realiza la operación.");
        }

        var usuario = ObtenerPorDni(dni);
        if (usuario == null)
        {
            throw new AccesoDenegadoException($"No existe ningún usuario registrado con el DNI '{dni.Trim()}'.");
        }

        return usuario;
    }

    /// <summary>Exige un DNI válido cuyo usuario tenga exactamente el rol pedido.</summary>
    public Usuario ValidarRol(string? dni, RolUsuario rolEsperado)
    {
        var usuario = ValidarUsuario(dni);

        if (usuario.Rol != rolEsperado)
        {
            throw new AccesoDenegadoException($"Acción no permitida: el usuario {usuario.Nombre} tiene rol '{usuario.Rol}' pero se requiere rol '{rolEsperado}'.");
        }

        return usuario;
    }
}
