using Api.Application.Features.Usuarios.GetUsuarioById;
using Api.Application.Features.Usuarios.UpdateUsuario;
using Api.FunctionalTests.Factories;
using Api.FunctionalTests.Infrastructure;
using Shouldly;
using System.Net;
using System.Net.Http.Json;

namespace Api.FunctionalTests.Features.Usuarios;

[TestFixture]
public sealed class UsuariosCrudEndpointTests : FunctionalTestFixture
{
    [Test]
    public async Task Actualizar_usuario_endpoint_debeRetornar_HttpOk()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(
                nombre: "Endpoint",
                apellido: "Original",
                email: "endpoint.original@example.com"
            )
        );
        var request = new UpdateUsuarioRequest(
            "Endpoint",
            "Actualizado",
            "endpoint.actualizado@example.com"
        );

        var response = await Client.PutAsJsonAsync($"/api/usuarios/{usuarioCreado.Id}", request);
        var content = await response.Content.ReadFromJsonAsync<UpdateUsuarioResponse>();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        content.ShouldNotBeNull();
        content.Id.ShouldBe(usuarioCreado.Id);
        content.Nombre.ShouldBe("Endpoint");
        content.Apellido.ShouldBe("Actualizado");
        content.Email.ShouldBe("endpoint.actualizado@example.com");
    }

    [Test]
    public async Task Actualizar_usuario_endpoint_con_id_inexistente_debeRetornar_HttpNotFound()
    {
        var request = new UpdateUsuarioRequest(
            "Endpoint",
            "Inexistente",
            "endpoint.inexistente@example.com"
        );

        var response = await Client.PutAsJsonAsync($"/api/usuarios/{Guid.NewGuid()}", request);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Eliminar_usuario_endpoint_debeRetornar_HttpNoContent()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(email: "endpoint.delete@example.com")
        );

        var response = await Client.DeleteAsync($"/api/usuarios/{usuarioCreado.Id}");
        var getResponse = await Client.GetAsync($"/api/usuarios/{usuarioCreado.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Eliminar_usuario_endpoint_con_id_inexistente_debeRetornar_HttpNotFound()
    {
        var response = await Client.DeleteAsync($"/api/usuarios/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Eliminar_usuario_endpoint_debe_permitir_recrear_email()
    {
        var usuarioCreado = await Sender.Send(
            UsuarioCommandFactory.Create(email: "endpoint.recrear@example.com")
        );
        await Client.DeleteAsync($"/api/usuarios/{usuarioCreado.Id}");

        var nuevoUsuario = await Sender.Send(
            UsuarioCommandFactory.Create(email: "ENDPOINT.RECREAR@example.com")
        );

        nuevoUsuario.Id.ShouldNotBe(usuarioCreado.Id);
        nuevoUsuario.Email.ShouldBe("endpoint.recrear@example.com");
    }
}
