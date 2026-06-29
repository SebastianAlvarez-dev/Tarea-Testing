using Api.Application.Features.Usuarios.CreateUsuario;
using Api.Application.Features.Usuarios.DeleteUsuario;
using Api.Application.Features.Usuarios.GetUsuarioById;
using Api.Application.Features.Usuarios.GetUsuarios;
using Api.Application.Features.Usuarios.UpdateUsuario;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly ISender _sender;

    public UsuariosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<GetUsuariosResponse>>> GetUsuarios(
        [FromQuery] GetUsuariosRequest request,
        CancellationToken cancellationToken
    )
    {
        var query = request.ToQuery();
        var validationErrors = GetUsuariosValidator.Validate(query);

        if (validationErrors.Count > 0)
            return ValidationProblem(new ValidationProblemDetails(validationErrors));

        var usuarios = await _sender.Send(query, cancellationToken);

        return Ok(usuarios);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUsuarioByIdResponse>> GetUsuarioById(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var usuario = await _sender.Send(new GetUsuarioByIdQuery(id), cancellationToken);

        if (usuario is null)
            return NotFound();

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<CreateUsuarioResponse>> CreateUsuario(
        CreateUsuarioRequest request,
        CancellationToken cancellationToken
    )
    {
        var usuario = await _sender.Send(request.ToCommand(), cancellationToken);

        return CreatedAtAction(nameof(GetUsuarioById), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UpdateUsuarioResponse>> UpdateUsuario(
        Guid id,
        UpdateUsuarioRequest request,
        CancellationToken cancellationToken
    )
    {
        var usuario = await _sender.Send(request.ToCommand(id), cancellationToken);

        if (usuario is null)
            return NotFound();

        return Ok(usuario);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteUsuario(Guid id, CancellationToken cancellationToken)
    {
        var eliminado = await _sender.Send(new DeleteUsuarioCommand(id), cancellationToken);

        if (!eliminado)
            return NotFound();

        return NoContent();
    }
}
