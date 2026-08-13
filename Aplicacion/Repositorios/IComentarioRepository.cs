using Aplicacion.Dominio;

namespace Aplicacion.Abstracciones;

public interface IComentarioRepository
{
    IReadOnlyList<Comentario> ObtenerComentariosConDetalles();
    void AgregarComentario(Comentario comentario);
}
