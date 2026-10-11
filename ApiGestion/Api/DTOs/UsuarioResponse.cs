using GestionEventos.Logica;

namespace GestionEventos.Api.DTOs;

public record UsuarioResponse(string Dni, string Nombre, string Username, RolUsuario Rol);
