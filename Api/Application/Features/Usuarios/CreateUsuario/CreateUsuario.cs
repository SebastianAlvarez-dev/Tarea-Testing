using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.CreateUsuario;

public sealed record CreateUsuarioRequest(string Nombre, string Apellido, string Email);

public sealed record CreateUsuarioCommand(string Nombre, string Apellido, string Email)
    : IRequest<CreateUsuarioResponse>;

public sealed record CreateUsuarioResponse(Guid Id, string Nombre, string Apellido, string Email);

public static class CreateUsuarioMapper
{
    public static CreateUsuarioCommand ToCommand(this CreateUsuarioRequest request)
    {
        return new CreateUsuarioCommand(request.Nombre, request.Apellido, request.Email);
    }

    public static CreateUsuarioResponse ToResponse(this Usuario usuario)
    {
        return new CreateUsuarioResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value
        );
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

        if (string.IsNullOrWhiteSpace(command.Email))
            throw new ArgumentException(
                "El email del usuario es requerido.",
                nameof(command.Email)
            );
    }
}

public sealed class CreateUsuarioCommandHandler
    : IRequestHandler<CreateUsuarioCommand, CreateUsuarioResponse>
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

        var email = Email.From(command.Email);
        var emailExists = await _context.Usuarios.AnyAsync(
            usuario => usuario.Email == email,
            cancellationToken
        );

        if (emailExists)
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var usuario = new Usuario(command.Nombre, command.Apellido, email);

        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return usuario.ToResponse();
    }
}
