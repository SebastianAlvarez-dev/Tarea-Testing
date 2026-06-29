using Api.Domain.Entities;
using Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.GetUsuarios;

public sealed record GetUsuariosQuery;

public sealed record GetUsuariosResponse(Guid Id, string Nombre, string Apellido);

public static class GetUsuariosMapper
{
    public static GetUsuariosResponse ToResponse(this Usuario usuario)
    {
        return new GetUsuariosResponse(usuario.Id, usuario.Nombre, usuario.Apellido);
    }
}

public static class GetUsuariosValidator
{
    public static void Validate(GetUsuariosQuery query) { }
}

public sealed class GetUsuariosQueryHandler
{
    private readonly ApplicationDbContext _context;

    public GetUsuariosQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<GetUsuariosResponse>> Handle(
        GetUsuariosQuery query,
        CancellationToken cancellationToken
    )
    {
        GetUsuariosValidator.Validate(query);

        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre)
            .ToArrayAsync(cancellationToken);

        return usuarios.Select(usuario => usuario.ToResponse()).ToArray();
    }
}
