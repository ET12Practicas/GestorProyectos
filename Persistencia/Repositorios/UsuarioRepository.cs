using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Persistencia;

internal sealed class UsuarioRepository(ProyectoDbContext context) : IUsuarioRepository
{
    public async Task<IReadOnlyList<Usuario>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default)
    {
        return await context.Usuarios
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Usuario?> ObtenerUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
    }

    public async Task<bool> ExisteUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await context.Usuarios.AnyAsync(u => u.Id == usuarioId, cancellationToken);
    }

    public void AgregarUsuario(Usuario usuario) => context.Usuarios.Add(usuario);

    public void EliminarUsuario(Usuario usuario) => context.Usuarios.Remove(usuario);
}
