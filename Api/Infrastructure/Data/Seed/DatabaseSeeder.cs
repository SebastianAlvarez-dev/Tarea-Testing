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

        if (await _context.Usuarios.AnyAsync(cancellationToken))
            return;

        var faker = new Faker("es")
        {
            Random = new Randomizer(20260628)
        };

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
}
