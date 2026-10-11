using GestionEventos.Api.DTOs;
using GestionEventos.Logica;
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
        return Ejecutar(() => Ok(_usuarioService.ObtenerTodos().Select(u => u.AResponse()).ToList()));
    }
}
