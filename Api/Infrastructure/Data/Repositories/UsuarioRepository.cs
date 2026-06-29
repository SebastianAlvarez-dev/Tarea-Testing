using Api.Application.Abstractions.Data;
using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Usuario usuario, CancellationToken cancellationToken)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AnyAsync(usuario => usuario.Email == email, cancellationToken);
    }

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre)
            .ToArrayAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
