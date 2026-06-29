using Api.Application.Abstractions.Data;
using Api.Domain.Entities;

namespace Api.Application.Features.Usuarios.GetUsuarios;

public sealed record GetUsuariosQuery;

public sealed record GetUsuariosResponse(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email
);

public static class GetUsuariosMapper
{
    public static GetUsuariosResponse ToResponse(this Usuario usuario)
    {
        return new GetUsuariosResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value);
    }
}

public static class GetUsuariosValidator
{
    public static void Validate(GetUsuariosQuery query)
    {
    }
}

public sealed class GetUsuariosQueryHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public GetUsuariosQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<IReadOnlyList<GetUsuariosResponse>> Handle(
        GetUsuariosQuery query,
        CancellationToken cancellationToken)
    {
        GetUsuariosValidator.Validate(query);

        var usuarios = await _usuarioRepository.ListAsync(cancellationToken);

        return usuarios
            .Select(usuario => usuario.ToResponse())
            .ToArray();
    }
}
