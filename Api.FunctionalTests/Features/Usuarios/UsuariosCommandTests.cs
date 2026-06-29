using Api.Application.Features.Usuarios.CreateUsuario;
using Api.FunctionalTests.Factories;
using Api.FunctionalTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Vogen;

namespace Api.FunctionalTests.Features.Usuarios;

[TestFixture]
public sealed class UsuariosCommandTests : FunctionalTestFixture
{
    [Test]
    public async Task CreateUsuario_persiste_usuario_en_base_de_datos()
    {
        var command = UsuarioCommandFactory.Create(
            nombre: "Sebastian",
            apellido: "Prueba",
            email: "SEBASTIAN.PRUEBA@example.com"
        );

        var usuario = await Sender.Send(command);

        var usuariosEnBase = await DbContext.Usuarios.AsNoTracking().ToArrayAsync();

        usuario.Id.ShouldNotBe(Guid.Empty);
        usuario.Nombre.ShouldBe("Sebastian");
        usuario.Apellido.ShouldBe("Prueba");
        usuario.Email.ShouldBe("sebastian.prueba@example.com");
        usuariosEnBase.Length.ShouldBe(1);
        usuariosEnBase[0].Email.Value.ShouldBe("sebastian.prueba@example.com");
    }

    [TestCase("")]
    [TestCase(" ")]
    public async Task CreateUsuario_con_nombre_vacio_debe_fallar_validacion(string nombre)
    {
        var command = UsuarioCommandFactory.Create(nombre: nombre);

        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await Sender.Send(command)
        );

        exception.ParamName.ShouldBe(nameof(CreateUsuarioCommand.Nombre));
    }

    [TestCase("")]
    [TestCase(" ")]
    public async Task CreateUsuario_con_apellido_vacio_debe_fallar_validacion(string apellido)
    {
        var command = UsuarioCommandFactory.Create(apellido: apellido);

        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await Sender.Send(command)
        );

        exception.ParamName.ShouldBe(nameof(CreateUsuarioCommand.Apellido));
    }

    [TestCase("")]
    [TestCase(" ")]
    public async Task CreateUsuario_con_email_vacio_debe_fallar_validacion(string email)
    {
        var command = UsuarioCommandFactory.Create(email: email);

        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await Sender.Send(command)
        );

        exception.ParamName.ShouldBe(nameof(CreateUsuarioCommand.Email));
    }

    [Test]
    public async Task CreateUsuario_con_email_invalido_debe_fallar()
    {
        var command = UsuarioCommandFactory.Create(email: "email-invalido");

        await Should.ThrowAsync<ValueObjectValidationException>(
            async () => await Sender.Send(command)
        );
    }

    [Test]
    public async Task CreateUsuario_con_email_duplicado_debe_fallar()
    {
        var command = UsuarioCommandFactory.Create(email: "duplicado@example.com");
        await Sender.Send(command);

        var duplicatedCommand = UsuarioCommandFactory.Create(email: "DUPLICADO@example.com");

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await Sender.Send(duplicatedCommand)
        );

        exception.Message.ShouldBe("Ya existe un usuario con ese email.");
    }
}
