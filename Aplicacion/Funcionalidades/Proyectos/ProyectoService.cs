using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Aplicacion.Funcionalidades.Comentarios;
using Aplicacion.Funcionalidades.Tickets;
using Aplicacion.Funcionalidades.Usuarios;

namespace Aplicacion.Funcionalidades.Proyectos;

public interface IProyectoService
{
    IReadOnlyList<ProyectoQueryDto> ObtenerProyectos();
    bool CrearProyecto(ProyectoCommandDto dto);
    bool CrearTicket(Guid proyectoId, TicketCommandDto dto);
    bool EliminarTicket(Guid ticketId);
    bool AsignarUsuarioATicket(Guid ticketId, Guid usuarioId);
    bool CambiarEstadoTicket(Guid ticketId, EstadoTicketDto dto);
    bool QuitarUsuarioDeProyectos(Guid usuarioId, Guid ticketId);
    bool AsignarUsuarioAProyecto(Guid usuarioId, Guid proyectoId);
    bool CrearComentario(Guid ticketId, Guid usuarioId, ComentarioCommandDto dto);
}

internal sealed class ProyectoService(
    IProyectoRepository proyectoRepository,
    ITicketRepository ticketRepository,
    IUsuarioRepository usuarioRepository,
    IComentarioRepository comentarioRepository,
    IUnidadDeTrabajo unidadDeTrabajo) : IProyectoService
{
    public IReadOnlyList<ProyectoQueryDto> ObtenerProyectos()
    {
        return proyectoRepository.ObtenerProyectosConDetalles()
            .Select(proyecto => new ProyectoQueryDto
            {
                Id = proyecto.IdProject,
                Nombre = proyecto.Nombre,
                Tickets = proyecto.Tickets.Select(TicketService.MapearTicket).ToList(),
                Usuarios = proyecto.Usuarios.Select(UsuarioService.MapearUsuario).ToList()
            })
            .ToList();
    }

    public bool CrearProyecto(ProyectoCommandDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return false;
        }

        proyectoRepository.AgregarProyecto(new Proyecto(dto.Nombre.Trim()));
        unidadDeTrabajo.GuardarCambios();
        return true;
    }

    public bool CrearTicket(Guid proyectoId, TicketCommandDto dto)
    {
        var proyecto = proyectoRepository.ObtenerProyecto(proyectoId);
        if (proyecto is null || string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return false;
        }

        var ticket = new Ticket(dto.Nombre.Trim());
        proyecto.Tickets.Add(ticket);
        ticketRepository.AgregarTicket(ticket);
        unidadDeTrabajo.GuardarCambios();
        return true;
    }

    public bool EliminarTicket(Guid ticketId)
    {
        var ticket = ticketRepository.ObtenerTicket(ticketId);
        if (ticket is null)
        {
            return false;
        }

        ticketRepository.EliminarTicket(ticket);
        unidadDeTrabajo.GuardarCambios();
        return true;
    }

    public bool AsignarUsuarioATicket(Guid ticketId, Guid usuarioId)
    {
        var ticket = ticketRepository.ObtenerTicket(ticketId);
        var usuario = usuarioRepository.ObtenerUsuario(usuarioId);
        if (ticket is null || usuario is null ||
            !proyectoRepository.ExisteProyectoConUsuarioYTicket(usuarioId, ticketId))
        {
            return false;
        }

        ticket.AgregarUsuario(usuario);
        unidadDeTrabajo.GuardarCambios();
        return true;
    }

    public bool CambiarEstadoTicket(Guid ticketId, EstadoTicketDto dto)
    {
        var ticket = ticketRepository.ObtenerTicket(ticketId);
        if (ticket is null || !Enum.IsDefined(dto.Estado))
        {
            return false;
        }

        ticket.Estado = dto.Estado;
        unidadDeTrabajo.GuardarCambios();
        return true;
    }

    public bool QuitarUsuarioDeProyectos(Guid usuarioId, Guid ticketId)
    {
        var usuario = usuarioRepository.ObtenerUsuario(usuarioId);
        var ticket = ticketRepository.ObtenerTicket(ticketId);
        var proyectos = proyectoRepository.ObtenerProyectosDelUsuario(usuarioId);
        if (usuario is null || ticket is null || proyectos.Count == 0)
        {
            return false;
        }

        if (ticket.UsuarioTicket?.Id == usuarioId)
        {
            ticket.UsuarioTicket = null;
        }

        foreach (var proyecto in proyectos)
        {
            proyecto.Usuarios.Remove(usuario);
        }

        unidadDeTrabajo.GuardarCambios();
        return true;
    }

    public bool AsignarUsuarioAProyecto(Guid usuarioId, Guid proyectoId)
    {
        var usuario = usuarioRepository.ObtenerUsuario(usuarioId);
        var proyecto = proyectoRepository.ObtenerProyecto(proyectoId);
        if (usuario is null || proyecto is null)
        {
            return false;
        }

        if (proyecto.Usuarios.All(item => item.Id != usuarioId))
        {
            proyecto.Usuarios.Add(usuario);
            unidadDeTrabajo.GuardarCambios();
        }

        return true;
    }

    public bool CrearComentario(
        Guid ticketId,
        Guid usuarioId,
        ComentarioCommandDto dto)
    {
        var ticket = ticketRepository.ObtenerTicket(ticketId);
        var usuario = usuarioRepository.ObtenerUsuario(usuarioId);
        if (ticket is null || usuario is null || string.IsNullOrWhiteSpace(dto.Contenido) ||
            !proyectoRepository.ExisteProyectoConUsuarioYTicket(usuarioId, ticketId))
        {
            return false;
        }

        var comentario = new Comentario(dto.Contenido.Trim())
        {
            UsuarioComentario = usuario
        };

        ticket.AgregarComentario(comentario);
        comentarioRepository.AgregarComentario(comentario);
        unidadDeTrabajo.GuardarCambios();
        return true;
    }
}
