using Aplicacion.Abstracciones;
using Aplicacion.Comun;
using Aplicacion.Dominio;
using Aplicacion.Enums;
using Aplicacion.Funcionalidades.Comentarios;
using Aplicacion.Funcionalidades.Usuarios;
using FluentValidation;

namespace Aplicacion.Funcionalidades.Tickets;

public interface ITicketService
{
    Task<IReadOnlyList<TicketQueryDto>> ObtenerTicketsAsync(CancellationToken cancellationToken = default);
    Task<TicketQueryDto?> ObtenerTicketPorIdAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TicketQueryDto>> ObtenerTicketsPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CrearTicketAsync(Guid proyectoId, TicketCommandDto dto, CancellationToken cancellationToken = default);
    Task<Result> CambiarEstadoTicketAsync(Guid ticketId, EstadoTicketDto dto, CancellationToken cancellationToken = default);
    Task<Result> AsignarUsuarioATicketAsync(Guid ticketId, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Result> DesasignarUsuarioDeTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<Result> EliminarTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
}

internal sealed class TicketService(
    ITicketRepository ticketRepository,
    IProyectoRepository proyectoRepository,
    IUsuarioRepository usuarioRepository,
    IUnidadDeTrabajo unidadDeTrabajo,
    IValidator<TicketCommandDto> validator) : ITicketService
{
    public async Task<IReadOnlyList<TicketQueryDto>> ObtenerTicketsAsync(CancellationToken cancellationToken = default)
    {
        var tickets = await ticketRepository.ObtenerTicketsConDetallesAsync(cancellationToken);
        return tickets.Select(MapearTicket).ToList();
    }

    public async Task<TicketQueryDto?> ObtenerTicketPorIdAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.ObtenerTicketConDetallesAsync(ticketId, cancellationToken);
        return ticket is null ? null : MapearTicket(ticket);
    }

    public async Task<IReadOnlyList<TicketQueryDto>> ObtenerTicketsPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        var tickets = await ticketRepository.ObtenerTicketsPorProyectoAsync(proyectoId, cancellationToken);
        return tickets.Select(MapearTicket).ToList();
    }

    public async Task<Result<Guid>> CrearTicketAsync(Guid proyectoId, TicketCommandDto dto, CancellationToken cancellationToken = default)
    {
        var validacion = await validator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
        {
            var primerError = validacion.Errors[0].ErrorMessage;
            return Result.Failure<Guid>(Error.Validation(primerError));
        }

        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        if (proyecto is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Proyecto no encontrado."));
        }

        var ticket = new Ticket(dto.Nombre.Trim(), dto.Descripcion?.Trim() ?? string.Empty)
        {
            ProyectoId = proyecto.Id,
            ProyectoTicket = proyecto
        };

        proyecto.Tickets.Add(ticket);
        ticketRepository.AgregarTicket(ticket);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Success(ticket.Id);
    }

    public async Task<Result> CambiarEstadoTicketAsync(Guid ticketId, EstadoTicketDto dto, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(dto.Estado))
        {
            return Result.Failure(Error.Validation("El estado proporcionado no es válido."));
        }

        var ticket = await ticketRepository.ObtenerTicketAsync(ticketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(Error.NotFound("Ticket no encontrado."));
        }

        ticket.Estado = dto.Estado;
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> AsignarUsuarioATicketAsync(Guid ticketId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.ObtenerTicketAsync(ticketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(Error.NotFound("Ticket no encontrado."));
        }

        var usuario = await usuarioRepository.ObtenerUsuarioAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure(Error.NotFound("Usuario no encontrado."));
        }

        var perteneceAlMismoProyecto = await proyectoRepository.ExisteProyectoConUsuarioYTicketAsync(usuarioId, ticketId, cancellationToken);
        if (!perteneceAlMismoProyecto)
        {
            return Result.Failure(Error.Conflict("El usuario debe pertenecer al proyecto del ticket antes de asignárselo."));
        }

        ticket.AgregarUsuario(usuario);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> DesasignarUsuarioDeTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.ObtenerTicketAsync(ticketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(Error.NotFound("Ticket no encontrado."));
        }

        ticket.UsuarioTicket = null;
        ticket.UsuarioId = null;
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> EliminarTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var ticket = await ticketRepository.ObtenerTicketAsync(ticketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure(Error.NotFound("Ticket no encontrado."));
        }

        ticketRepository.EliminarTicket(ticket);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    internal static TicketQueryDto MapearTicket(Ticket ticket)
    {
        return new TicketQueryDto
        {
            Id = ticket.Id,
            Nombre = ticket.Nombre,
            Descripcion = ticket.Descripcion,
            Estado = ticket.Estado,
            Fecha = ticket.FechaTicket,
            Usuario = ticket.UsuarioTicket is null
                ? null
                : UsuarioService.MapearUsuario(ticket.UsuarioTicket),
            Comentarios = ticket.Comentarios
                .Select(ComentarioService.MapearComentario)
                .ToList()
        };
    }
}
