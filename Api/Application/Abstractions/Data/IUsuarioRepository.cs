using Api.Domain.Entities;
using Api.Domain.ValueObjects;

namespace Api.Application.Abstractions.Data;

public interface IUsuarioRepository
{
    Task AddAsync(Usuario usuario, CancellationToken cancellationToken);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken);
    Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Usuario>> ListAsync(CancellationToken cancellationToken);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
