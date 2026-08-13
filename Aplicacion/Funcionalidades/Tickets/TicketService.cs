using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Aplicacion.Funcionalidades.Comentarios;
using Aplicacion.Funcionalidades.Usuarios;

namespace Aplicacion.Funcionalidades.Tickets;

public interface ITicketService
{
    IReadOnlyList<TicketQueryDto> ObtenerTickets();
}

internal sealed class TicketService(ITicketRepository ticketRepository) : ITicketService
{
    public IReadOnlyList<TicketQueryDto> ObtenerTickets()
    {
        return ticketRepository.ObtenerTicketsConDetalles().Select(MapearTicket).ToList();
    }

    internal static TicketQueryDto MapearTicket(Ticket ticket)
    {
        return new TicketQueryDto
        {
            Id = ticket.Id,
            Nombre = ticket.Nombre,
            Estado = ticket.Estado,
            Fecha = ticket.FechaTicket,
            Usuario = ticket.UsuarioTicket is null
                ? null
                : UsuarioService.MapearUsuario(ticket.UsuarioTicket),
            Comentario = ticket.ComentarioTicket is null
                ? null
                : ComentarioService.MapearComentario(ticket.ComentarioTicket)
        };
    }
}
