using Api.Application.Features.Usuarios.CreateUsuario;
using Api.Application.Features.Usuarios.GetUsuarios;
using Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.FunctionalTests;

[TestFixture]
public sealed class UsuariosFeatureTests : FunctionalTestFixture
{
    [Test]
    public async Task CreateUsuario_persiste_usuario_en_base_de_datos()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var usuario = await sender.Send(
            new CreateUsuarioCommand("Sebastian", "Prueba", "SEBASTIAN.PRUEBA@example.com")
        );

        var usuariosEnBase = await context.Usuarios.AsNoTracking().ToArrayAsync();

        Assert.That(usuario.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(usuario.Nombre, Is.EqualTo("Sebastian"));
        Assert.That(usuario.Apellido, Is.EqualTo("Prueba"));
        Assert.That(usuario.Email, Is.EqualTo("sebastian.prueba@example.com"));
        Assert.That(usuariosEnBase, Has.Length.EqualTo(1));
        Assert.That(usuariosEnBase[0].Email.Value, Is.EqualTo("sebastian.prueba@example.com"));
    }

    [Test]
    public async Task GetUsuarios_devuelve_resultado_paginado()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await sender.Send(new CreateUsuarioCommand("Ana", "Zamora", "ana@example.com"));
        await sender.Send(new CreateUsuarioCommand("Luis", "Alvarez", "luis@example.com"));
        await sender.Send(new CreateUsuarioCommand("Maria", "Bonilla", "maria@example.com"));

        var pagina = await sender.Send(new GetUsuariosQuery(PageNumber: 1, PageSize: 2));

        Assert.That(pagina.TotalCount, Is.EqualTo(3));
        Assert.That(pagina.TotalPages, Is.EqualTo(2));
        Assert.That(pagina.Items, Has.Count.EqualTo(2));
        Assert.That(pagina.HasPreviousPage, Is.False);
        Assert.That(pagina.HasNextPage, Is.True);
        Assert.That(pagina.Items[0].Apellido, Is.EqualTo("Alvarez"));
        Assert.That(pagina.Items[1].Apellido, Is.EqualTo("Bonilla"));
    }

    [Test]
    public void GetUsuarios_con_paginacion_invalida_falla_validacion()
    {
        using var scope = CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var exception = Assert.ThrowsAsync<ArgumentException>(async () =>
            await sender.Send(new GetUsuariosQuery(PageNumber: 0, PageSize: 101))
        );

        Assert.That(exception!.Message, Is.EqualTo("La paginacion solicitada no es valida."));
    }
}
