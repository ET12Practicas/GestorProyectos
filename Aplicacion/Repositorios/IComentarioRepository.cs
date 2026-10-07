using Aplicacion.Dominio;

namespace Aplicacion.Abstracciones;

public interface IComentarioRepository
{
    Task<IReadOnlyList<Comentario>> ObtenerComentariosConDetallesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Comentario>> ObtenerComentariosPorTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<Comentario?> ObtenerComentarioAsync(Guid comentarioId, CancellationToken cancellationToken = default);
    void AgregarComentario(Comentario comentario);
    void EliminarComentario(Comentario comentario);
}
