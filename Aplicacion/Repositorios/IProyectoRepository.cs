using Aplicacion.Dominio;

namespace Aplicacion.Abstracciones;

public interface IProyectoRepository
{
    IReadOnlyList<Proyecto> ObtenerProyectosConDetalles();
    Proyecto? ObtenerProyecto(Guid proyectoId);
    IReadOnlyList<Proyecto> ObtenerProyectosDelUsuario(Guid usuarioId);
    bool ExisteProyectoConUsuarioYTicket(Guid usuarioId, Guid ticketId);
    void AgregarProyecto(Proyecto proyecto);
}
