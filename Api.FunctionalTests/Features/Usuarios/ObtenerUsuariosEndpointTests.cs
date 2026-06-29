using Api.Application.Features.Usuarios.GetUsuarios;
using Api.FunctionalTests.Factories;
using Api.FunctionalTests.Infrastructure;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Api.FunctionalTests.Features.Usuarios;

[TestFixture]
public sealed class ObtenerUsuariosEndpointTests : FunctionalTestFixture
{
    [Test]
    public async Task Obtener_usuarios_endpoint_debeRetornar_HttpOk()
    {
        await Sender.Send(UsuarioCommandFactory.Create(email: "ana.endpoint@example.com"));

        var response = await Client.GetAsync("/api/usuarios?pageNumber=1&pageSize=10");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task Obtener_usuarios_endpoint_debeRetornar_respuesta_paginada()
    {
        await Sender.Send(
            UsuarioCommandFactory.Create(nombre: "Ana", apellido: "Zamora", email: "ana.zamora@example.com")
        );
        await Sender.Send(
            UsuarioCommandFactory.Create(nombre: "Luis", apellido: "Alvarez", email: "luis.alvarez@example.com")
        );
        await Sender.Send(
            UsuarioCommandFactory.Create(nombre: "Maria", apellido: "Bonilla", email: "maria.bonilla@example.com")
        );

        var response = await Client.GetAsync("/api/usuarios?pageNumber=1&pageSize=2");
        var content = await response.Content.ReadFromJsonAsync<PaginatedResponse<GetUsuariosResponse>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        content.ShouldNotBeNull();
        content.PageNumber.ShouldBe(1);
        content.PageSize.ShouldBe(2);
        content.TotalCount.ShouldBe(3);
        content.TotalPages.ShouldBe(2);
        content.HasPreviousPage.ShouldBeFalse();
        content.HasNextPage.ShouldBeTrue();
        content.Items.Count.ShouldBe(2);
        content.Items[0].Apellido.ShouldBe("Alvarez");
        content.Items[1].Apellido.ShouldBe("Bonilla");
    }

    [Test]
    public async Task Obtener_usuarios_endpoint_sin_query_params_debeUsar_paginacion_por_defecto()
    {
        await Sender.Send(UsuarioCommandFactory.Create(email: "default.page@example.com"));

        var response = await Client.GetAsync("/api/usuarios");
        var content = await response.Content.ReadFromJsonAsync<PaginatedResponse<GetUsuariosResponse>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        content.ShouldNotBeNull();
        content.PageNumber.ShouldBe(1);
        content.PageSize.ShouldBe(10);
        content.TotalCount.ShouldBe(1);
        content.Items.Count.ShouldBe(1);
    }

    [Test]
    public async Task Obtener_usuarios_endpoint_sin_usuarios_debeRetornar_lista_vacia()
    {
        var response = await Client.GetAsync("/api/usuarios?pageNumber=1&pageSize=10");
        var content = await response.Content.ReadFromJsonAsync<PaginatedResponse<GetUsuariosResponse>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        content.ShouldNotBeNull();
        content.TotalCount.ShouldBe(0);
        content.TotalPages.ShouldBe(0);
        content.Items.ShouldBeEmpty();
        content.HasPreviousPage.ShouldBeFalse();
        content.HasNextPage.ShouldBeFalse();
    }

    [Test]
    public async Task Obtener_usuarios_endpoint_con_segunda_pagina_debeRetornar_HasPreviousPage()
    {
        await Sender.Send(UsuarioCommandFactory.Create(email: "pagina1@example.com"));
        await Sender.Send(UsuarioCommandFactory.Create(email: "pagina2@example.com"));
        await Sender.Send(UsuarioCommandFactory.Create(email: "pagina3@example.com"));

        var response = await Client.GetAsync("/api/usuarios?pageNumber=2&pageSize=2");
        var content = await response.Content.ReadFromJsonAsync<PaginatedResponse<GetUsuariosResponse>>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        content.ShouldNotBeNull();
        content.PageNumber.ShouldBe(2);
        content.PageSize.ShouldBe(2);
        content.TotalCount.ShouldBe(3);
        content.TotalPages.ShouldBe(2);
        content.Items.Count.ShouldBe(1);
        content.HasPreviousPage.ShouldBeTrue();
        content.HasNextPage.ShouldBeFalse();
    }

    [TestCase(0, 10)]
    [TestCase(1, 0)]
    [TestCase(1, 101)]
    public async Task Obtener_usuarios_endpoint_con_paginacion_invalida_debeRetornar_HttpBadRequest(
        int pageNumber,
        int pageSize
    )
    {
        var response = await Client.GetAsync(
            $"/api/usuarios?pageNumber={pageNumber}&pageSize={pageSize}"
        );

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}
