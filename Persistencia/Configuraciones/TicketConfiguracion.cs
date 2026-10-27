using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistencia.Configuraciones;

public sealed class TicketConfiguracion : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Ticket");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Nombre)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Descripcion)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.FechaTicket)
            .IsRequired();

        builder.Property(t => t.Estado)
            .IsRequired();

        builder.HasOne(t => t.UsuarioTicket)
            .WithMany()
            .HasForeignKey(t => t.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(t => t.ProyectoTicket)
            .WithMany(p => p.Tickets)
            .HasForeignKey(t => t.ProyectoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Comentarios)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
