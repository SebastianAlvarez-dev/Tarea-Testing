using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.UpdateUsuario;

public sealed record UpdateUsuarioRequest(string Nombre, string Apellido, string Email);

public sealed record UpdateUsuarioCommand(Guid Id, string Nombre, string Apellido, string Email)
    : IRequest<UpdateUsuarioResponse?>;

public sealed record UpdateUsuarioResponse(Guid Id, string Nombre, string Apellido, string Email);

public static class UpdateUsuarioMapper
{
    public static UpdateUsuarioCommand ToCommand(this UpdateUsuarioRequest request, Guid id)
    {
        return new UpdateUsuarioCommand(id, request.Nombre, request.Apellido, request.Email);
    }

    public static UpdateUsuarioResponse ToUpdateResponse(this Usuario usuario)
    {
        return new UpdateUsuarioResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value
        );
    }
}

public static class UpdateUsuarioValidator
{
    public static void Validate(UpdateUsuarioCommand command)
    {
        if (command.Id == Guid.Empty)
            throw new ArgumentException("El id del usuario es requerido.", nameof(command.Id));

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

public sealed class UpdateUsuarioCommandHandler
    : IRequestHandler<UpdateUsuarioCommand, UpdateUsuarioResponse?>
{
    private readonly ApplicationDbContext _context;

    public UpdateUsuarioCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateUsuarioResponse?> Handle(
        UpdateUsuarioCommand command,
        CancellationToken cancellationToken
    )
    {
        UpdateUsuarioValidator.Validate(command);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(
            usuario => usuario.Id == command.Id,
            cancellationToken
        );

        if (usuario is null)
            return null;

        var email = Email.From(command.Email);
        var emailExists = await _context.Usuarios.AnyAsync(
            usuario => usuario.Id != command.Id && usuario.Email == email,
            cancellationToken
        );

        if (emailExists)
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        usuario.Actualizar(command.Nombre, command.Apellido, email);

        await _context.SaveChangesAsync(cancellationToken);

        return usuario.ToUpdateResponse();
    }
}
