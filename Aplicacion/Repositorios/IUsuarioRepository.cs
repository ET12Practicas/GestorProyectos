using Aplicacion.Dominio;

namespace Aplicacion.Abstracciones;

public interface IUsuarioRepository
{
    IReadOnlyList<Usuario> ObtenerUsuarios();
    Usuario? ObtenerUsuario(Guid usuarioId);
    void AgregarUsuario(Usuario usuario);
    void EliminarUsuario(Usuario usuario);
}
