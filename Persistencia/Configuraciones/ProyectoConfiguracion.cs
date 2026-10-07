using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistencia.Configuraciones;

public sealed class ProyectoConfiguracion : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> builder)
    {
        builder.ToTable("Proyecto");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Nombre)
            .IsRequired()
            .HasMaxLength(45);

        builder.HasMany(p => p.Usuarios)
            .WithMany(u => u.Proyectos)
            .UsingEntity(join => join.ToTable("ProyectoUsuario"));

        builder.HasMany(p => p.Tickets)
            .WithOne(t => t.ProyectoTicket)
            .HasForeignKey(t => t.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
