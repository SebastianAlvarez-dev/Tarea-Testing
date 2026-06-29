using Api.Application.Features.Usuarios.GetUsuarioById;
using Api.Application.Features.Usuarios.DeleteUsuario;
using Api.Application.Features.Usuarios.UpdateUsuario;
using Api.FunctionalTests.Factories;
using Api.FunctionalTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Api.FunctionalTests.Features.Usuarios;

[TestFixture]
public sealed class UsuariosUpdateCommandTests : FunctionalTestFixture
{
    [Test]
    public async Task UpdateUsuario_actualiza_usuario_en_base_de_datos()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(
                nombre: "Nombre Original",
                apellido: "Apellido Original",
                email: "original.update@example.com"
            )
        );

        var response = await Sender.Send(
            new UpdateUsuarioCommand(
                usuarioCreado.Id,
                "Nombre Editado",
                "Apellido Editado",
                "editado.update@example.com"
            )
        );

        var usuarioEnBase = await DbContext
            .Usuarios.AsNoTracking()
            .SingleAsync(usuario => usuario.Id == usuarioCreado.Id);

        response.ShouldNotBeNull();
        response.Id.ShouldBe(usuarioCreado.Id);
        response.Nombre.ShouldBe("Nombre Editado");
        response.Apellido.ShouldBe("Apellido Editado");
        response.Email.ShouldBe("editado.update@example.com");
        usuarioEnBase.Nombre.ShouldBe("Nombre Editado");
        usuarioEnBase.Apellido.ShouldBe("Apellido Editado");
        usuarioEnBase.Email.Value.ShouldBe("editado.update@example.com");
    }

    [Test]
    public async Task UpdateUsuario_con_id_inexistente_debe_retornar_null()
    {
        var response = await Sender.Send(
            new UpdateUsuarioCommand(
                Guid.NewGuid(),
                "Nombre",
                "Apellido",
                "inexistente.update@example.com"
            )
        );

        response.ShouldBeNull();
    }

    [Test]
    public async Task UpdateUsuario_con_email_duplicado_debe_fallar()
    {
        await Sender.Send(UsuarioCommandFactory.Create(email: "ocupado.update@example.com"));
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(email: "libre.update@example.com")
        );

        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () =>
                await Sender.Send(
                    new UpdateUsuarioCommand(
                        usuarioCreado.Id,
                        "Nombre",
                        "Apellido",
                        "OCUPADO.UPDATE@example.com"
                    )
                )
        );

        exception.Message.ShouldBe("Ya existe un usuario con ese email.");
    }

    [TestCase("")]
    [TestCase(" ")]
    public async Task UpdateUsuario_con_nombre_vacio_debe_fallar_validacion(string nombre)
    {
        var command = new UpdateUsuarioCommand(
            Guid.NewGuid(),
            nombre,
            "Apellido",
            "nombre.vacio.update@example.com"
        );

        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await Sender.Send(command)
        );

        exception.ParamName.ShouldBe(nameof(UpdateUsuarioCommand.Nombre));
    }

    [Test]
    public async Task UpdateUsuario_con_id_vacio_debe_fallar_validacion()
    {
        var command = new UpdateUsuarioCommand(
            Guid.Empty,
            "Nombre",
            "Apellido",
            "id.vacio.update@example.com"
        );

        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await Sender.Send(command)
        );

        exception.ParamName.ShouldBe(nameof(UpdateUsuarioCommand.Id));
    }

    [Test]
    public async Task UpdateUsuario_no_debe_actualizar_usuario_eliminado()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(email: "eliminado.update@example.com")
        );
        await Sender.Send(new DeleteUsuarioCommand(usuarioCreado.Id));

        var response = await Sender.Send(
            new UpdateUsuarioCommand(
                usuarioCreado.Id,
                "Nombre",
                "Apellido",
                "nuevo.eliminado.update@example.com"
            )
        );

        var usuario = await Sender.Send(new GetUsuarioByIdQuery(usuarioCreado.Id));

        response.ShouldBeNull();
        usuario.ShouldBeNull();
    }
}
