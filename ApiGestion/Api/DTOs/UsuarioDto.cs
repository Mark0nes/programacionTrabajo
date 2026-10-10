namespace GestionEventos.Api.DTOs;
using GestionEventos.Logica;

public record UsuarioDto(string Dni, string Nombre, string Username, RolUsuario Rol);