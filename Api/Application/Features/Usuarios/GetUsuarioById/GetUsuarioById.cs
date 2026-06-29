using Api.Application.Abstractions.Data;
using Api.Domain.Entities;

namespace Api.Application.Features.Usuarios.GetUsuarioById;

public sealed record GetUsuarioByIdQuery(Guid Id);

public sealed record GetUsuarioByIdResponse(
    Guid Id,
    string Nombre,
    string Apellido,
    string Email
);

public static class GetUsuarioByIdMapper
{
    public static GetUsuarioByIdResponse ToResponse(this Usuario usuario)
    {
        return new GetUsuarioByIdResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value);
    }
}

public static class GetUsuarioByIdValidator
{
    public static void Validate(GetUsuarioByIdQuery query)
    {
        if (query.Id == Guid.Empty)
            throw new ArgumentException("El id del usuario es requerido.", nameof(query.Id));
    }
}

public sealed class GetUsuarioByIdQueryHandler
{
    private readonly IUsuarioRepository _usuarioRepository;

    public GetUsuarioByIdQueryHandler(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<GetUsuarioByIdResponse?> Handle(
        GetUsuarioByIdQuery query,
        CancellationToken cancellationToken)
    {
        GetUsuarioByIdValidator.Validate(query);

        var usuario = await _usuarioRepository.GetByIdAsync(query.Id, cancellationToken);

        return usuario?.ToResponse();
    }
}
