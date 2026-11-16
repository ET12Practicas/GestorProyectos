using Dominio.Entidades;

namespace Aplicacion.Abstracciones;

public interface IUsuarioRepository
{
    Task<IReadOnlyList<Usuario>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerUsuarioConRelacionesAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerUsuarioPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default);
    void EliminarUsuario(Usuario usuario);
}
