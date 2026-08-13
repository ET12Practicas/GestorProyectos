using Aplicacion.Dominio;

namespace Aplicacion.Abstracciones;

public interface ITicketRepository
{
    IReadOnlyList<Ticket> ObtenerTicketsConDetalles();
    Ticket? ObtenerTicket(Guid ticketId);
    void AgregarTicket(Ticket ticket);
    void EliminarTicket(Ticket ticket);
}
