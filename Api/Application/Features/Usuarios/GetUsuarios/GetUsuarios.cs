using Api.Domain.Entities;
using Api.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Api.Application.Features.Usuarios.GetUsuarios;

public sealed record GetUsuariosRequest(int PageNumber = 1, int PageSize = 10);

public sealed record GetUsuariosQuery(int PageNumber, int PageSize)
    : IRequest<PaginatedResponse<GetUsuariosResponse>>;

public sealed record GetUsuariosResponse(Guid Id, string Nombre, string Apellido, string Email);

public sealed record PaginatedResponse<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages,
    bool HasPreviousPage,
    bool HasNextPage
);

public static class GetUsuariosMapper
{
    public static GetUsuariosQuery ToQuery(this GetUsuariosRequest request)
    {
        return new GetUsuariosQuery(request.PageNumber, request.PageSize);
    }

    public static GetUsuariosResponse ToResponse(this Usuario usuario)
    {
        return new GetUsuariosResponse(
            usuario.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Email.Value
        );
    }
}

public static class GetUsuariosValidator
{
    private const int MaxPageSize = 100;

    public static Dictionary<string, string[]> Validate(GetUsuariosQuery query)
    {
        var errors = new Dictionary<string, string[]>();

        if (query.PageNumber < 1)
        {
            errors[nameof(query.PageNumber)] = ["El numero de pagina debe ser mayor o igual a 1."];
        }

        if (query.PageSize < 1 || query.PageSize > MaxPageSize)
        {
            errors[nameof(query.PageSize)] =
            [
                $"El tamanio de pagina debe estar entre 1 y {MaxPageSize}.",
            ];
        }

        return errors;
    }
}

public sealed class GetUsuariosQueryHandler
    : IRequestHandler<GetUsuariosQuery, PaginatedResponse<GetUsuariosResponse>>
{
    private readonly ApplicationDbContext _context;

    public GetUsuariosQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponse<GetUsuariosResponse>> Handle(
        GetUsuariosQuery query,
        CancellationToken cancellationToken
    )
    {
        var validationErrors = GetUsuariosValidator.Validate(query);

        if (validationErrors.Count > 0)
            throw new ArgumentException("La paginacion solicitada no es valida.");

        var totalCount = await _context.Usuarios.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

        var usuarios = await _context
            .Usuarios.AsNoTracking()
            .OrderBy(usuario => usuario.Apellido)
            .ThenBy(usuario => usuario.Nombre)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);

        var items = usuarios.Select(usuario => usuario.ToResponse()).ToArray();

        return new PaginatedResponse<GetUsuariosResponse>(
            items,
            query.PageNumber,
            query.PageSize,
            totalCount,
            totalPages,
            query.PageNumber > 1,
            query.PageNumber < totalPages
        );
    }
}
