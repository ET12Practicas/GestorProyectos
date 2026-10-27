using Dominio.Entidades;

namespace Aplicacion.Abstracciones;

public interface IProyectoRepository
{
    Task<IReadOnlyList<Proyecto>> ObtenerProyectosConDetallesAsync(CancellationToken cancellationToken = default);
    Task<Proyecto?> ObtenerProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Proyecto>> ObtenerProyectosDelUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<bool> ExisteProyectoConUsuarioYTicketAsync(Guid usuarioId, Guid ticketId, CancellationToken cancellationToken = default);
    Task<bool> ExisteProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    void AgregarProyecto(Proyecto proyecto);
    void EliminarProyecto(Proyecto proyecto);
}
