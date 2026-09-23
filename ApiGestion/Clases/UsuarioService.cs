namespace GestionEventos.Logica;

using GestionEventos.Data;

public class UsuarioService
{
    private readonly PersonaRepository _personaRepository;

    public UsuarioService(PersonaRepository? personaRepository = null)
    {
        _personaRepository = personaRepository ?? new PersonaRepository();
    }

    public List<Usuario> ObtenerTodos()
    {
        return _personaRepository.ObtenerUsuarios();
    }

    public Usuario? ObtenerPorDni(string? dni)
    {
        if (string.IsNullOrWhiteSpace(dni)) return null;
        string dniLimpio = dni.Trim();
        return _personaRepository.ObtenerUsuarios().FirstOrDefault(u => u.Dni.Equals(dniLimpio, StringComparison.OrdinalIgnoreCase));
    }

    public Usuario ValidarRol(string? dni, RolUsuario rolEsperado)
    {
        if (string.IsNullOrWhiteSpace(dni))
        {
            throw new UnauthorizedAccessException("Se requiere especificar el DNI del usuario que realiza la operación.");
        }

        var usuario = ObtenerPorDni(dni);
        if (usuario == null)
        {
            throw new UnauthorizedAccessException($"No existe ningún usuario registrado con el DNI '{dni}'.");
        }

        if (usuario.Rol != rolEsperado)
        {
            throw new UnauthorizedAccessException($"Acción no permitida: el usuario {usuario.Nombre} tiene rol '{usuario.Rol}' pero se requiere rol '{rolEsperado}'.");
        }

        return usuario;
    }
}
