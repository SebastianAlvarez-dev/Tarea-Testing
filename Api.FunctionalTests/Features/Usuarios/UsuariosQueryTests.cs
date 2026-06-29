using Api.Application.Features.Usuarios.GetUsuarioById;
using Api.Application.Features.Usuarios.GetUsuarios;
using Api.FunctionalTests.Factories;
using Api.FunctionalTests.Infrastructure;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Api.FunctionalTests.Features.Usuarios;

[TestFixture]
public sealed class UsuariosQueryTests : FunctionalTestFixture
{
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
    public async Task GetUsuarios_con_pagina_fuera_de_rango_debe_retornar_items_vacios()
    {
        await Sender.Send(UsuarioCommandFactory.Create(email: "uno@example.com"));

        var pagina = await Sender.Send(new GetUsuariosQuery(PageNumber: 2, PageSize: 10));

        pagina.TotalCount.ShouldBe(1);
        pagina.TotalPages.ShouldBe(1);
        pagina.Items.ShouldBeEmpty();
        pagina.HasPreviousPage.ShouldBeTrue();
        pagina.HasNextPage.ShouldBeFalse();
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(1, 101)]
    public async Task GetUsuarios_con_paginacion_invalida_debe_fallar_validacion(
        int pageNumber,
        int pageSize
    )
    {
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await Sender.Send(new GetUsuariosQuery(pageNumber, pageSize))
        );

        exception.Message.ShouldBe("La paginacion solicitada no es valida.");
    }

    [Test]
    public async Task GetUsuarioById_con_id_existente_debe_retornar_usuario()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(
                nombre: "Paula",
                apellido: "Query",
                email: "paula.query@example.com"
            )
        );

        var usuario = await Sender.Send(new GetUsuarioByIdQuery(usuarioCreado.Id));

        usuario.ShouldNotBeNull();
        usuario.Id.ShouldBe(usuarioCreado.Id);
        usuario.Nombre.ShouldBe("Paula");
        usuario.Apellido.ShouldBe("Query");
        usuario.Email.ShouldBe("paula.query@example.com");
    }

    [Test]
    public async Task GetUsuarioById_con_id_inexistente_debe_retornar_null()
    {
        var usuario = await Sender.Send(new GetUsuarioByIdQuery(Guid.NewGuid()));

        usuario.ShouldBeNull();
    }

    [Test]
    public async Task GetUsuarioById_con_id_vacio_debe_fallar_validacion()
    {
        var exception = await Should.ThrowAsync<ArgumentException>(
            async () => await Sender.Send(new GetUsuarioByIdQuery(Guid.Empty))
        );

        exception.ParamName.ShouldBe(nameof(GetUsuarioByIdQuery.Id));
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
        var content = await response.Content.ReadFromJsonAsync<GetUsuarioByIdResponse>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        content.ShouldNotBeNull();
        content.Id.ShouldBe(usuario.Id);
        content.Nombre.ShouldBe("Paula");
        content.Apellido.ShouldBe("Controller");
        content.Email.ShouldBe("paula.controller@example.com");
    }

    [Test]
    public async Task Obtener_usuario_controller_con_id_inexistente_debeRetornar_HttpNotFound()
    {
        var response = await Client.GetAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
