using Aplicacion.Dominio;

namespace Aplicacion.Abstracciones;

public interface IUsuarioRepository
{
    Task<IReadOnlyList<Usuario>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<bool> ExisteUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    void AgregarUsuario(Usuario usuario);
    void EliminarUsuario(Usuario usuario);
}
