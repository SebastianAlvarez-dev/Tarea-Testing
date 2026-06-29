using Api.Domain.Entities;
using Api.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Data.Configurations;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(usuario => usuario.Id);

        builder.Property(usuario => usuario.Nombre).HasMaxLength(100).IsRequired();

        builder.Property(usuario => usuario.Apellido).HasMaxLength(100).IsRequired();

        builder.Property(usuario => usuario.IsDeleted).IsRequired();

        builder
            .Property(usuario => usuario.Email)
            .HasConversion(email => email.Value, value => Email.From(value))
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(usuario => usuario.Email).IsUnique().HasFilter("[IsDeleted] = 0");

        builder.HasQueryFilter(usuario => !usuario.IsDeleted);
    }
}
