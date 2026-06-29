using Api.Application.Features.Usuarios.CreateUsuario;
using Bogus;

namespace Api.FunctionalTests.Factories;

public static class UsuarioCommandFactory
{
    private static readonly Faker Faker = new("es");

    public static CreateUsuarioCommand Create(
        string? nombre = null,
        string? apellido = null,
        string? email = null
    )
    {
        var generatedNombre = nombre ?? Faker.Name.FirstName();
        var generatedApellido = apellido ?? Faker.Name.LastName();
        var generatedEmail =
            email ?? Faker.Internet.Email(generatedNombre, generatedApellido).ToLowerInvariant();

        return new CreateUsuarioCommand(generatedNombre, generatedApellido, generatedEmail);
    }
}
