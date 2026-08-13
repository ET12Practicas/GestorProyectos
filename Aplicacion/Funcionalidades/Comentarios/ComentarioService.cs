using Aplicacion.Abstracciones;

namespace Aplicacion.Funcionalidades.Comentarios;

public interface IComentarioService
{
    IReadOnlyList<ComentarioQueryDto> ObtenerComentarios();
}

internal sealed class ComentarioService(IComentarioRepository comentarioRepository)
    : IComentarioService
{
    public IReadOnlyList<ComentarioQueryDto> ObtenerComentarios()
    {
        return comentarioRepository.ObtenerComentariosConDetalles()
            .Select(MapearComentario)
            .ToList();
    }

    internal static ComentarioQueryDto MapearComentario(Dominio.Comentario comentario)
    {
        return new ComentarioQueryDto
        {
            Id = comentario.IdComentario,
            Contenido = comentario.Contenido,
            Fecha = comentario.FechaComentario,
            Usuario = comentario.UsuarioComentario is null
                ? null
                : Usuarios.UsuarioService.MapearUsuario(comentario.UsuarioComentario)
        };
    }
}
