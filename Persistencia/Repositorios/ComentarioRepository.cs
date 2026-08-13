using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Persistencia;

internal sealed class ComentarioRepository(ProyectoDbContext context) : IComentarioRepository
{
    public IReadOnlyList<Comentario> ObtenerComentariosConDetalles()
    {
        return context.Comentarios
            .AsNoTracking()
            .Include(comentario => comentario.UsuarioComentario)
            .ToList();
    }

    public void AgregarComentario(Comentario comentario) =>
        context.Comentarios.Add(comentario);
}
