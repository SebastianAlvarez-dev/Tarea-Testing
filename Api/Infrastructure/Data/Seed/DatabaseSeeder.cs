using Api.Domain.Entities;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data.Seed;

public sealed class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;

    public DatabaseSeeder(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.EnsureCreatedAsync(cancellationToken);
        await RemoveLegacyEmailColumnAsync(cancellationToken);

        if (await _context.Usuarios.AnyAsync(cancellationToken))
            return;

        var faker = new Faker("es") { Random = new Randomizer(20260628) };

        var usuarios = Enumerable
            .Range(1, 20)
            .Select(_ =>
            {
                var nombre = faker.Name.FirstName();
                var apellido = faker.Name.LastName();

                return new Usuario(nombre, apellido);
            })
            .ToArray();

        await _context.Usuarios.AddRangeAsync(usuarios, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task RemoveLegacyEmailColumnAsync(CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlRawAsync(
            """
            IF EXISTS (
                SELECT 1
                FROM sys.indexes
                WHERE name = N'IX_Usuarios_Email'
                  AND object_id = OBJECT_ID(N'[Usuarios]')
            )
            BEGIN
                DROP INDEX [IX_Usuarios_Email] ON [Usuarios];
            END;

            IF COL_LENGTH(N'[Usuarios]', N'Email') IS NOT NULL
            BEGIN
                ALTER TABLE [Usuarios] DROP COLUMN [Email];
            END;
            """,
            cancellationToken
        );
    }
}
