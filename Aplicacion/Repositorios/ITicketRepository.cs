using Aplicacion.Dominio;

namespace Aplicacion.Abstracciones;

public interface ITicketRepository
{
    Task<IReadOnlyList<Ticket>> ObtenerTicketsConDetallesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Ticket>> ObtenerTicketsPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    Task<Ticket?> ObtenerTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<Ticket?> ObtenerTicketConDetallesAsync(Guid ticketId, CancellationToken cancellationToken = default);
    void AgregarTicket(Ticket ticket);
    void EliminarTicket(Ticket ticket);
}
