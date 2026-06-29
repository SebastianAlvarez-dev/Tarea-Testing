using Api.Application.Features.Usuarios.CreateUsuario;
using Api.Application.Features.Usuarios.GetUsuarioById;
using Api.Application.Features.Usuarios.GetUsuarios;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly CreateUsuarioCommandHandler _createUsuarioCommandHandler;
    private readonly GetUsuarioByIdQueryHandler _getUsuarioByIdQueryHandler;
    private readonly GetUsuariosQueryHandler _getUsuariosQueryHandler;

    public UsuariosController(
        CreateUsuarioCommandHandler createUsuarioCommandHandler,
        GetUsuarioByIdQueryHandler getUsuarioByIdQueryHandler,
        GetUsuariosQueryHandler getUsuariosQueryHandler)
    {
        _createUsuarioCommandHandler = createUsuarioCommandHandler;
        _getUsuarioByIdQueryHandler = getUsuarioByIdQueryHandler;
        _getUsuariosQueryHandler = getUsuariosQueryHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GetUsuariosResponse>>> GetUsuarios(
        CancellationToken cancellationToken)
    {
        var usuarios = await _getUsuariosQueryHandler.Handle(new GetUsuariosQuery(), cancellationToken);

        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUsuarioByIdResponse>> GetUsuarioById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var usuario = await _getUsuarioByIdQueryHandler.Handle(
            new GetUsuarioByIdQuery(id),
            cancellationToken);

        if (usuario is null)
            return NotFound();

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<CreateUsuarioResponse>> CreateUsuario(
        CreateUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _createUsuarioCommandHandler.Handle(request.ToCommand(), cancellationToken);

        return CreatedAtAction(nameof(GetUsuarioById), new { id = usuario.Id }, usuario);
    }
}
