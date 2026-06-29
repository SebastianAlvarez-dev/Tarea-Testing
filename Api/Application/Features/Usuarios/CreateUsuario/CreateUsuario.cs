using Api.Application.Abstractions.Data;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Features.Usuarios.CreateUsuario;

public sealed record CreateUsuarioRequest(
    string Nombre,
    string Apellido,
    string Email
);

public sealed record CreateUsuarioCommand(
    string Nombre,
    string Apellido,
    string Email
);

public sealed record CreateUsuarioResponse(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email
);

public static class CreateUsuarioMapper
{
    public static CreateUsuarioCommand ToCommand(this CreateUsuarioRequest request)
    {
        return new CreateUsuarioCommand(
            request.Nombre,
            request.Apellido,
            request.Email);
    }

    public static CreateUsuarioResponse ToResponse(this Usuario usuario)
    {
        return new CreateUsuarioResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value);
    }
}

public static class CreateUsuarioValidator
{
    public static void Validate(CreateUsuarioCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nombre))
            throw new ArgumentException("El nombre del usuario es requerido.", nameof(command.Nombre));

        if (string.IsNullOrWhiteSpace(command.Apellido))
            throw new ArgumentException("El apellido del usuario es requerido.", nameof(command.Apellido));

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException("El email del usuario es requerido.", nameof(command.Email));
    }
}

public sealed class CreateUsuarioCommandHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public CreateUsuarioCommandHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<CreateUsuarioResponse> Handle(
        CreateUsuarioCommand command,
        CancellationToken cancellationToken)
    {
        CreateUsuarioValidator.Validate(command);

        var email = Email.From(command.Email);

        if (await _usuarioRepository.ExistsByEmailAsync(email, cancellationToken))
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var usuario = new Usuario(command.Nombre, command.Apellido, email);

        await _usuarioRepository.AddAsync(usuario, cancellationToken);
        await _usuarioRepository.SaveChangesAsync(cancellationToken);

        return usuario.ToResponse();
    }
}
