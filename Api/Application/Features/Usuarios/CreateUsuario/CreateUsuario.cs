using Api.Domain.Entities;
using Api.Infrastructure.Data;

namespace Api.Application.Features.Usuarios.CreateUsuario;

public sealed record CreateUsuarioRequest(string Nombre, string Apellido);

public sealed record CreateUsuarioCommand(string Nombre, string Apellido);

public sealed record CreateUsuarioResponse(Guid Id, string Nombre, string Apellido);

public static class CreateUsuarioMapper
{
    public static CreateUsuarioCommand ToCommand(this CreateUsuarioRequest request)
    {
        return new CreateUsuarioCommand(request.Nombre, request.Apellido);
    }

    public static CreateUsuarioResponse ToResponse(this Usuario usuario)
    {
        return new CreateUsuarioResponse(usuario.Id, usuario.Nombre, usuario.Apellido);
    }
}

public static class CreateUsuarioValidator
{
    public static void Validate(CreateUsuarioCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nombre))
            throw new ArgumentException(
                "El nombre del usuario es requerido.",
                nameof(command.Nombre)
            );

        if (string.IsNullOrWhiteSpace(command.Apellido))
            throw new ArgumentException(
                "El apellido del usuario es requerido.",
                nameof(command.Apellido)
            );
    }
}

public sealed class CreateUsuarioCommandHandler
{
    private readonly ApplicationDbContext _context;

    public CreateUsuarioCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateUsuarioResponse> Handle(
        CreateUsuarioCommand command,
        CancellationToken cancellationToken
    )
    {
        CreateUsuarioValidator.Validate(command);

        var usuario = new Usuario(command.Nombre, command.Apellido);

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return usuario.ToResponse();
    }
}
