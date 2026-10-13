using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Persistencia;

internal sealed class ProyectoRepository(ProyectoDbContext context) : IProyectoRepository
{
    public async Task<IReadOnlyList<Proyecto>> ObtenerProyectosConDetallesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Proyectos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(proyecto => proyecto.Creador)
            .Include(proyecto => proyecto.Usuarios)
            .Include(proyecto => proyecto.Tickets)
                .ThenInclude(ticket => ticket.UsuarioTicket)
            .Include(proyecto => proyecto.Tickets)
                .ThenInclude(ticket => ticket.Comentarios)
                    .ThenInclude(comentario => comentario.UsuarioComentario)
            .ToListAsync(cancellationToken);
    }

    public async Task<Proyecto?> ObtenerProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        return await context.Proyectos
            .Include(proyecto => proyecto.Creador)
            .Include(proyecto => proyecto.Usuarios)
            .Include(proyecto => proyecto.Tickets)
            .FirstOrDefaultAsync(proyecto => proyecto.Id == proyectoId, cancellationToken);
    }

    public async Task<IReadOnlyList<Proyecto>> ObtenerProyectosDelUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await context.Proyectos
            .Include(proyecto => proyecto.Creador)
            .Include(proyecto => proyecto.Usuarios)
            .Where(proyecto => proyecto.CreadorId == usuarioId || proyecto.Usuarios.Any(usuario => usuario.Id == usuarioId))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteProyectoConUsuarioYTicketAsync(Guid usuarioId, Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await context.Proyectos.AnyAsync(proyecto =>
            proyecto.Usuarios.Any(usuario => usuario.Id == usuarioId) &&
            proyecto.Tickets.Any(ticket => ticket.Id == ticketId), cancellationToken);
    }

    public async Task<bool> ExisteProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        return await context.Proyectos.AnyAsync(p => p.Id == proyectoId, cancellationToken);
    }

    public void AgregarProyecto(Proyecto proyecto) => context.Proyectos.Add(proyecto);

    public void EliminarProyecto(Proyecto proyecto) => context.Proyectos.Remove(proyecto);
}
