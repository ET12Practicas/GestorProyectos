using Dominio.Enums;
using Aplicacion.Funcionalidades.Comentarios;
using Aplicacion.Funcionalidades.Usuarios;

namespace Aplicacion.Funcionalidades.Tickets;

public sealed class TicketCommandDto
{
    public required string Nombre { get; set; }
    public string Descripcion { get; set; } = string.Empty;
}

public sealed class EstadoTicketDto
{
    public EEstadoTicket Estado { get; set; }
}

public sealed class TicketQueryDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public UsuarioQueryDto? Usuario { get; set; }
    public EEstadoTicket Estado { get; set; } = EEstadoTicket.Abierto;
    public List<ComentarioQueryDto> Comentarios { get; set; } = [];
    public DateTime Fecha { get; set; }
}
