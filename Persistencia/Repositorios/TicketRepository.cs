using Aplicacion.Abstracciones;
using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;

namespace Persistencia;

internal sealed class TicketRepository(ProyectoDbContext context) : ITicketRepository
{
    public async Task<IReadOnlyList<Ticket>> ObtenerTicketsConDetallesAsync(CancellationToken cancellationToken = default)
    {
        return await context.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.UsuarioTicket)
            .Include(ticket => ticket.ProyectoTicket)
            .Include(ticket => ticket.Comentarios)
                .ThenInclude(comentario => comentario.UsuarioComentario)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> ObtenerTicketsPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        return await context.Tickets
            .AsNoTracking()
            .Where(ticket => ticket.ProyectoId == proyectoId)
            .Include(ticket => ticket.UsuarioTicket)
            .Include(ticket => ticket.Comentarios)
                .ThenInclude(comentario => comentario.UsuarioComentario)
            .ToListAsync(cancellationToken);
    }

    public async Task<Ticket?> ObtenerTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await context.Tickets
            .Include(ticket => ticket.UsuarioTicket)
            .FirstOrDefaultAsync(ticket => ticket.Id == ticketId, cancellationToken);
    }

    public async Task<Ticket?> ObtenerTicketConDetallesAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        return await context.Tickets
            .Include(ticket => ticket.UsuarioTicket)
            .Include(ticket => ticket.ProyectoTicket)
            .Include(ticket => ticket.Comentarios)
                .ThenInclude(comentario => comentario.UsuarioComentario)
            .FirstOrDefaultAsync(ticket => ticket.Id == ticketId, cancellationToken);
    }

    public void AgregarTicket(Ticket ticket) => context.Tickets.Add(ticket);

    public void EliminarTicket(Ticket ticket) => context.Tickets.Remove(ticket);
}
