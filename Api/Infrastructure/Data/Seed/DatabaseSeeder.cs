using Api.Domain.Entities;
using Api.Domain.ValueObjects;
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
        await EnsureEmailColumnAsync(cancellationToken);
        await EnsureSoftDeleteColumnAsync(cancellationToken);

        if (await _context.Usuarios.AnyAsync(cancellationToken))
            return;

        var faker = new Faker("es") { Random = new Randomizer(20260628) };

        var usuarios = Enumerable
            .Range(1, 20)
            .Select(index =>
            {
                var nombre = faker.Name.FirstName();
                var apellido = faker.Name.LastName();
                var email = Email.From($"usuario{index}@example.com");

                return new Usuario(nombre, apellido, email);
            })
            .ToArray();

        await _context.Usuarios.AddRangeAsync(usuarios, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureEmailColumnAsync(CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[Usuarios]', N'U') IS NOT NULL
               AND COL_LENGTH(N'[Usuarios]', N'Email') IS NULL
            BEGIN
                ALTER TABLE [Usuarios] ADD [Email] nvarchar(256) NULL;

                EXEC(N'
                    WITH NumberedUsuarios AS
                    (
                        SELECT [Id], ROW_NUMBER() OVER (ORDER BY [Apellido], [Nombre], [Id]) AS RowNumber
                        FROM [Usuarios]
                    )
                    UPDATE u
                    SET [Email] = CONCAT(''usuario'', n.RowNumber, ''@example.com'')
                    FROM [Usuarios] u
                    INNER JOIN NumberedUsuarios n ON u.[Id] = n.[Id];
                ');

                ALTER TABLE [Usuarios] ALTER COLUMN [Email] nvarchar(256) NOT NULL;
            END;

            """,
            cancellationToken
        );
    }

    private async Task EnsureSoftDeleteColumnAsync(CancellationToken cancellationToken)
    {
        await _context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[Usuarios]', N'U') IS NOT NULL
               AND COL_LENGTH(N'[Usuarios]', N'IsDeleted') IS NULL
            BEGIN
                ALTER TABLE [Usuarios] ADD [IsDeleted] bit NOT NULL CONSTRAINT [DF_Usuarios_IsDeleted] DEFAULT CAST(0 AS bit);
            END;

            IF OBJECT_ID(N'[Usuarios]', N'U') IS NOT NULL
               AND EXISTS (
                   SELECT 1
                   FROM sys.indexes
                   WHERE name = N'IX_Usuarios_Email'
                     AND object_id = OBJECT_ID(N'[Usuarios]')
                     AND (filter_definition IS NULL OR filter_definition <> N'([IsDeleted]=(0))')
               )
            BEGIN
                DROP INDEX [IX_Usuarios_Email] ON [Usuarios];
            END;

            IF OBJECT_ID(N'[Usuarios]', N'U') IS NOT NULL
               AND NOT EXISTS (
                   SELECT 1
                   FROM sys.indexes
                   WHERE name = N'IX_Usuarios_Email'
                     AND object_id = OBJECT_ID(N'[Usuarios]')
               )
            BEGIN
                CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]) WHERE [IsDeleted] = 0;
            END;
            """,
            cancellationToken
        );
    }
}
