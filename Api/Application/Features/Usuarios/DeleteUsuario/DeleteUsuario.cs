using Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.DeleteUsuario;

public sealed record DeleteUsuarioCommand(Guid Id) : IRequest<bool>;

public static class DeleteUsuarioValidator
{
    public static void Validate(DeleteUsuarioCommand command)
    {
        if (command.Id == Guid.Empty)
            throw new ArgumentException("El id del usuario es requerido.", nameof(command.Id));
    }
}

public sealed class DeleteUsuarioCommandHandler : IRequestHandler<DeleteUsuarioCommand, bool>
{
    private readonly ApplicationDbContext _context;

    public DeleteUsuarioCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteUsuarioCommand command, CancellationToken cancellationToken)
    {
        DeleteUsuarioValidator.Validate(command);

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(
            usuario => usuario.Id == command.Id,
            cancellationToken
        );

        if (usuario is null)
            return false;

        usuario.Eliminar();

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
