using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Persistencia;

internal sealed class TicketRepository(ProyectoDbContext context) : ITicketRepository
{
    public IReadOnlyList<Ticket> ObtenerTicketsConDetalles()
    {
        return context.Tickets
            .AsNoTracking()
            .Include(ticket => ticket.UsuarioTicket)
            .Include(ticket => ticket.ComentarioTicket)
                .ThenInclude(comentario => comentario!.UsuarioComentario)
            .ToList();
    }

    public Ticket? ObtenerTicket(Guid ticketId)
    {
        return context.Tickets
            .Include(ticket => ticket.UsuarioTicket)
            .FirstOrDefault(ticket => ticket.Id == ticketId);
    }

    public void AgregarTicket(Ticket ticket) => context.Tickets.Add(ticket);

    public void EliminarTicket(Ticket ticket) => context.Tickets.Remove(ticket);
}
