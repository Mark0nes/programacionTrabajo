namespace GestionEventos.Logica;

public class Usuario
{
    public string Dni { get; private set; }
    public string Nombre { get; private set; }
    public string Username { get; private set; }
    public RolUsuario Rol { get; private set; }

    public Usuario(string dni, string nombre, string username, RolUsuario rol)
    {
        if (string.IsNullOrWhiteSpace(dni) || (dni.Length != 8))
        {
            throw new ArgumentException("El formato del dni es incorrecto");
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre no puede estar vacío o contener espacios en blanco");
        }

        if (string.IsNullOrWhiteSpace(username) && (username.Length < 6 || username.Length > 20))
        {
            throw new ArgumentException("El username no puede estar vacío o contener espacios en blanco, además debe contener entre 6 y 20 caracteres");
        }

        if (rol != RolUsuario.Organizador && rol != RolUsuario.Comprador)
        {
            throw new ArgumentException("El rol no existe, debe ser 'Organizador' o 'Comprador'");
        }

        Dni = dni;
        Nombre = nombre;
        Username = username;
        Rol = rol;
    }

    public void EsOrganizador()
    {
        if (Rol != RolUsuario.Organizador)
        {
            throw new ArgumentException("El usuario no es un organizador.");
        }
    }

    public void EsComprador()
    {
        if (Rol != RolUsuario.Comprador)
        {
            throw new ArgumentException("El usuario no es un comprador.");
        }
    }
}
