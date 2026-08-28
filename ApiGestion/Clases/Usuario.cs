namespace GestionEventos.Logica;

public class Usuario
{
    public int Dni { get; private set; }
    public string Nombre { get; private set; }
    public string Username { get; private set; }
    public RolUsuario Rol { get; private set; }

    public Usuario(int dni, string nombre, string username, RolUsuario rol)
    {
        Dni = dni;
        Nombre = nombre;
        Username = username;
        Rol = rol;
    }

    public void EsOrganizador()
    {
        if (Rol != RolUsuario.Organizador)
        {
            throw new Exception("El usuario no es un organizador.");
        }
    }

    public void EsComprador()
    {
        if (Rol != RolUsuario.Comprador)
        {
            throw new Exception("El usuario no es un comprador.");
        }
    }
}
