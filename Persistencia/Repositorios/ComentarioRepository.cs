using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Persistencia;

internal sealed class ComentarioRepository(ProyectoDbContext context) : IComentarioRepository
{
    public async Task<IReadOnlyList<Comentario>> ObtenerComentariosConDetallesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Comentarios
            .AsNoTracking()
            .Include(comentario => comentario.UsuarioComentario)
            .Include(comentario => comentario.Ticket)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Comentario>> ObtenerComentariosPorTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await context.Comentarios
            .AsNoTracking()
            .Where(c => c.TicketId == ticketId)
            .Include(c => c.UsuarioComentario)
            .OrderByDescending(c => c.FechaComentario)
            .ToListAsync(cancellationToken);
    }

    public async Task<Comentario?> ObtenerComentarioAsync(Guid comentarioId, CancellationToken cancellationToken = default)
    {
        return await context.Comentarios
            .Include(c => c.UsuarioComentario)
            .FirstOrDefaultAsync(c => c.Id == comentarioId, cancellationToken);
    }

    public void AgregarComentario(Comentario comentario) =>
        context.Comentarios.Add(comentario);

    public void EliminarComentario(Comentario comentario) =>
        context.Comentarios.Remove(comentario);
}
