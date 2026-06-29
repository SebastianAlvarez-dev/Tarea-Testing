using Api.Application.Features.Usuarios.GetUsuarios;
using Api.FunctionalTests.Factories;
using Api.FunctionalTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System.Net;

namespace Api.FunctionalTests.Features.Usuarios;

[TestFixture]
public sealed class UsuariosFeatureTests : FunctionalTestFixture
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

    [Test]
    public async Task GetUsuarios_devuelve_resultado_paginado()
    {
        await Sender.Send(
            UsuarioCommandFactory.Create(
                nombre: "Ana",
                apellido: "Zamora",
                email: "ana@example.com"
            )
        );
        await Sender.Send(
            UsuarioCommandFactory.Create(
                nombre: "Luis",
                apellido: "Alvarez",
                email: "luis@example.com"
            )
        );
        await Sender.Send(
            UsuarioCommandFactory.Create(
                nombre: "Maria",
                apellido: "Bonilla",
                email: "maria@example.com"
            )
        );

        var pagina = await Sender.Send(new GetUsuariosQuery(PageNumber: 1, PageSize: 2));

        pagina.TotalCount.ShouldBe(3);
        pagina.TotalPages.ShouldBe(2);
        pagina.Items.Count.ShouldBe(2);
        pagina.HasPreviousPage.ShouldBeFalse();
        pagina.HasNextPage.ShouldBeTrue();
        pagina.Items[0].Apellido.ShouldBe("Alvarez");
        pagina.Items[1].Apellido.ShouldBe("Bonilla");
    }

    [Test]
    public async Task GetUsuarios_con_paginacion_invalida_falla_validacion()
    {
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await Sender.Send(new GetUsuariosQuery(PageNumber: 0, PageSize: 101))
        );

        exception.Message.ShouldBe("La paginacion solicitada no es valida.");
    }

    [Test]
    public async Task Obtener_usuario_controller_debeRetornar_HttpOk()
    {
        var usuario = await Sender.Send(
            UsuarioCommandFactory.Create(
                nombre: "Paula",
                apellido: "Controller",
                email: "paula.controller@example.com"
            )
        );

        var response = await Client.GetAsync($"/api/usuarios/{usuario.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
