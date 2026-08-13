using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Persistencia;

internal sealed class ProyectoRepository(ProyectoDbContext context) : IProyectoRepository
{
    public IReadOnlyList<Proyecto> ObtenerProyectosConDetalles()
    {
        return context.Proyectos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(proyecto => proyecto.Usuarios)
            .Include(proyecto => proyecto.Tickets)
                .ThenInclude(ticket => ticket.UsuarioTicket)
            .Include(proyecto => proyecto.Tickets)
                .ThenInclude(ticket => ticket.ComentarioTicket)
                    .ThenInclude(comentario => comentario!.UsuarioComentario)
            .ToList();
    }

    public Proyecto? ObtenerProyecto(Guid proyectoId)
    {
        return context.Proyectos
            .Include(proyecto => proyecto.Usuarios)
            .Include(proyecto => proyecto.Tickets)
            .FirstOrDefault(proyecto => proyecto.IdProject == proyectoId);
    }

    public IReadOnlyList<Proyecto> ObtenerProyectosDelUsuario(Guid usuarioId)
    {
        return context.Proyectos
            .Include(proyecto => proyecto.Usuarios)
            .Where(proyecto => proyecto.Usuarios.Any(usuario => usuario.Id == usuarioId))
            .ToList();
    }

    public bool ExisteProyectoConUsuarioYTicket(Guid usuarioId, Guid ticketId)
    {
        return context.Proyectos.Any(proyecto =>
            proyecto.Usuarios.Any(usuario => usuario.Id == usuarioId) &&
            proyecto.Tickets.Any(ticket => ticket.Id == ticketId));
    }

    public void AgregarProyecto(Proyecto proyecto) => context.Proyectos.Add(proyecto);
}
