using Api.Application.Features.Usuarios.DeleteUsuario;
using Api.Application.Features.Usuarios.GetUsuarioById;
using Api.Application.Features.Usuarios.GetUsuarios;
using Api.FunctionalTests.Factories;
using Api.FunctionalTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace Api.FunctionalTests.Features.Usuarios;

[TestFixture]
public sealed class UsuariosDeleteCommandTests : FunctionalTestFixture
{
    [Test]
    public async Task DeleteUsuario_debe_marcar_usuario_como_eliminado()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(email: "soft.delete@example.com")
        );

        var eliminado = await Sender.Send(new DeleteUsuarioCommand(usuarioCreado.Id));

        var usuarioEliminado = await DbContext
            .Usuarios.IgnoreQueryFilters()
            .AsNoTracking()
            .SingleAsync(usuario => usuario.Id == usuarioCreado.Id);

        eliminado.ShouldBeTrue();
        usuarioEliminado.IsDeleted.ShouldBeTrue();
    }

    [Test]
    public async Task DeleteUsuario_debe_ocultar_usuario_en_consultas()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(email: "oculto.delete@example.com")
        );

        await Sender.Send(new DeleteUsuarioCommand(usuarioCreado.Id));

        var usuario = await Sender.Send(new GetUsuarioByIdQuery(usuarioCreado.Id));
        var pagina = await Sender.Send(new GetUsuariosQuery(PageNumber: 1, PageSize: 10));

        usuario.ShouldBeNull();
        pagina.TotalCount.ShouldBe(0);
        pagina.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task DeleteUsuario_con_id_inexistente_debe_retornar_false()
    {
        var eliminado = await Sender.Send(new DeleteUsuarioCommand(Guid.NewGuid()));

        eliminado.ShouldBeFalse();
    }

    [Test]
    public async Task DeleteUsuario_con_id_vacio_debe_fallar_validacion()
    {
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await Sender.Send(new DeleteUsuarioCommand(Guid.Empty))
        );

        exception.ParamName.ShouldBe(nameof(DeleteUsuarioCommand.Id));
    }

    [Test]
    public async Task CreateUsuario_debe_permitir_email_de_usuario_eliminado()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(email: "reutilizable.delete@example.com")
        );
        await Sender.Send(new DeleteUsuarioCommand(usuarioCreado.Id));

        var nuevoUsuario = await Sender.Send(
            UsuarioCommandFactory.Create(email: "REUTILIZABLE.DELETE@example.com")
        );

        nuevoUsuario.Id.ShouldNotBe(usuarioCreado.Id);
        nuevoUsuario.Email.ShouldBe("reutilizable.delete@example.com");
    }
}
