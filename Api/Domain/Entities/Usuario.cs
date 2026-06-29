using Api.Domain.Common;
using Api.Domain.ValueObjects;

namespace Api.Domain.Entities;

public sealed class Usuario : Entity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;
    public Email Email { get; private set; }
    public bool IsDeleted { get; private set; }

    private Usuario() { }

    public Usuario(string nombre, string apellido, Email email)
        : base(Guid.NewGuid())
    {
        CambiarNombre(nombre);
        CambiarApellido(apellido);
        CambiarEmail(email);
    }

    public void CambiarNombre(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre del usuario es requerido.", nameof(nombre));

        Nombre = nombre.Trim();
    }

    public void CambiarApellido(string apellido)
    {
        if (string.IsNullOrWhiteSpace(apellido))
            throw new ArgumentException("El apellido del usuario es requerido.", nameof(apellido));

        Apellido = apellido.Trim();
    }

    public void CambiarEmail(Email email)
    {
        Email = email;
    }

    public void Actualizar(string nombre, string apellido, Email email)
    {
        CambiarNombre(nombre);
        CambiarApellido(apellido);
        CambiarEmail(email);
    }

    public void Eliminar()
    {
        IsDeleted = true;
    }
}
