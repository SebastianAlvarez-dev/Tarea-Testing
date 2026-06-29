using Api.Application.Features.Usuarios.CreateUsuario;
using Api.Application.Features.Usuarios.GetUsuarioById;
using Api.Application.Features.Usuarios.GetUsuarios;

namespace Api.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateUsuarioCommandHandler>();
        services.AddScoped<GetUsuarioByIdQueryHandler>();
        services.AddScoped<GetUsuariosQueryHandler>();

        return services;
    }
}
