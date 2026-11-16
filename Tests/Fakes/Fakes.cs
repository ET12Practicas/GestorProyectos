using Aplicacion.Abstracciones;
using Dominio.Entidades;

namespace Tests.Fakes;

internal sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UsuarioId { get; set; }
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated => UsuarioId.HasValue;
    public bool EsAdministrador { get; set; }
}

internal sealed class FakeUnidadDeTrabajo : IUnidadDeTrabajo
{
    public int Guardados { get; private set; }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        Guardados++;
        return Task.FromResult(1);
    }
}

internal sealed class FakeUsuarioRepository : IUsuarioRepository
{
    public List<Usuario> Usuarios { get; } = [];
    public List<Guid> Eliminados { get; } = [];

    public Task<IReadOnlyList<Usuario>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Usuario>>(Usuarios.ToList());

    public Task<Usuario?> ObtenerUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == usuarioId));

    public Task<Usuario?> ObtenerUsuarioConRelacionesAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Usuarios.FirstOrDefault(u => u.Id == usuarioId));

    public Task<Usuario?> ObtenerUsuarioPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default) =>
        Task.FromResult(Usuarios.FirstOrDefault(u =>
            string.Equals(u.NormalizedEmail, emailNormalizado, StringComparison.OrdinalIgnoreCase)));

    public void EliminarUsuario(Usuario usuario) => Eliminados.Add(usuario.Id);
}

internal sealed class FakeProyectoRepository : IProyectoRepository
{
    public List<Proyecto> Proyectos { get; } = [];
    public List<Guid> Eliminados { get; } = [];

    public Task<IReadOnlyList<Proyecto>> ObtenerProyectosConDetallesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Proyecto>>(Proyectos.ToList());

    public Task<Proyecto?> ObtenerProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Proyectos.FirstOrDefault(p => p.Id == proyectoId));

    public Task<IReadOnlyList<Proyecto>> ObtenerProyectosDelUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Proyecto>>(Proyectos
            .Where(p => p.CreadorId == usuarioId || p.Usuarios.Any(u => u.Id == usuarioId))
            .ToList());

    public Task<bool> EsMiembroDelProyectoAsync(Guid proyectoId, Guid usuarioId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Proyectos.Any(p =>
            p.Id == proyectoId && (p.CreadorId == usuarioId || p.Usuarios.Any(u => u.Id == usuarioId))));

    public Task<bool> ExisteProyectoConUsuarioYTicketAsync(Guid usuarioId, Guid ticketId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Proyectos.Any(p =>
            p.Usuarios.Any(u => u.Id == usuarioId) && p.Tickets.Any(t => t.Id == ticketId)));

    public void AgregarProyecto(Proyecto proyecto) => Proyectos.Add(proyecto);

    public void EliminarProyecto(Proyecto proyecto) => Eliminados.Add(proyecto.Id);
}

internal sealed class FakeTicketRepository : ITicketRepository
{
    public List<Ticket> Tickets { get; } = [];
    public List<Guid> Eliminados { get; } = [];

    public Task<IReadOnlyList<Ticket>> ObtenerTicketsConDetallesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Ticket>>(Tickets.ToList());

    public Task<IReadOnlyList<Ticket>> ObtenerTicketsPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Ticket>>(Tickets.Where(t => t.ProyectoId == proyectoId).ToList());

    public Task<IReadOnlyList<Ticket>> ObtenerTicketsDelUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Ticket>>(Tickets.Where(t => t.UsuarioId == usuarioId).ToList());

    public Task<Ticket?> ObtenerTicketAsync(Guid ticketId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tickets.FirstOrDefault(t => t.Id == ticketId));

    public Task<Ticket?> ObtenerTicketConDetallesAsync(Guid ticketId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Tickets.FirstOrDefault(t => t.Id == ticketId));

    public void AgregarTicket(Ticket ticket) => Tickets.Add(ticket);

    public void EliminarTicket(Ticket ticket) => Eliminados.Add(ticket.Id);
}
