using Api.Domain.Entities;
using Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.GetUsuarioById;

public sealed record GetUsuarioByIdQuery(Guid Id) : IRequest<GetUsuarioByIdResponse?>;

public sealed record GetUsuarioByIdResponse(Guid Id, string Nombre, string Apellido, string Email);

public static class GetUsuarioByIdMapper
{
    public static GetUsuarioByIdResponse ToResponse(this Usuario usuario)
    {
        return new GetUsuarioByIdResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value
        );
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
    : IRequestHandler<GetUsuarioByIdQuery, GetUsuarioByIdResponse?>
{
    private readonly ApplicationDbContext _context;

    public GetUsuarioByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<GetUsuarioByIdResponse?> Handle(
        GetUsuarioByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        GetUsuarioByIdValidator.Validate(query);

        var usuario = await _context
            .Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(usuario => usuario.Id == query.Id, cancellationToken);

        return usuario?.ToResponse();
    }
}
