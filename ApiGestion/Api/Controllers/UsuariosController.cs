using GestionEventos.Logica;
using GestionEventos.Api.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace GestionEventos.Api.Controllers;

[Route("api/[controller]")]
public class UsuariosController : BaseApiController
{
    private readonly UsuarioService _usuarioService;

    public UsuariosController(UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public IActionResult ObtenerTodos()
    {
        var usuarios = _usuarioService.ObtenerTodos();
        var usuarioDtos = usuarios.Select(usuario => new UsuarioDto(
            usuario.Dni,
            usuario.Nombre,
            usuario.Username,
            usuario.Rol)).ToList();
        return Ok(usuarioDtos);
    }
}