using Api.Domain.Common;

namespace Api.Domain.Entities;

public sealed class Usuario : Entity
{
    public string Nombre { get; private set; }
    public string Apellido { get; private set; }
    public string Email { get; private set; }

    private Usuario()
    {
        Nombre = string.Empty;
        Apellido = string.Empty;
        Email = string.Empty;
    }

    public Usuario(string nombre, string apellido, string email)
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

    public void CambiarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email del usuario es requerido.", nameof(email));

        Email = email.Trim().ToLowerInvariant();
    }
}
