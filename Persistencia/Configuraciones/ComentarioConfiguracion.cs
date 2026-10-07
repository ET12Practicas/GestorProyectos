using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistencia.Configuraciones;

public sealed class ComentarioConfiguracion : IEntityTypeConfiguration<Comentario>
{
    public void Configure(EntityTypeBuilder<Comentario> builder)
    {
        builder.ToTable("Comentario");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Contenido)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(c => c.FechaComentario)
            .IsRequired();

        builder.HasOne(c => c.UsuarioComentario)
            .WithMany()
            .HasForeignKey(c => c.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Ticket)
            .WithMany(t => t.Comentarios)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
