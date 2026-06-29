using Api.Domain.Common;

namespace Api.Domain.Entities;

public sealed class Usuario : Entity
{
    public string Nombre { get; private set; } = string.Empty;
    public string Apellido { get; private set; } = string.Empty;

    private Usuario() { }

    public Usuario(string nombre, string apellido)
        : base(Guid.NewGuid())
    {
        CambiarNombre(nombre);
        CambiarApellido(apellido);
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
}
