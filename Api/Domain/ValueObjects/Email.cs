using Vogen;

namespace Api.Domain.ValueObjects;

[ValueObject<string>]
public readonly partial struct Email
{
    private static Validation Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Validation.Invalid("El email es requerido.");

        if (!value.Contains('@'))
            return Validation.Invalid("El email no tiene un formato valido.");

        return Validation.Ok;
    }

    private static string NormalizeInput(string value)
    {
        return value.Trim().ToLowerInvariant();
    }
}
