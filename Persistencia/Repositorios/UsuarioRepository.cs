using Aplicacion.Abstracciones;
using Dominio.Entidades;
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

    public async Task<Usuario?> ObtenerUsuarioConRelacionesAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await context.Usuarios
            .Include(u => u.Proyectos)
            .Include(u => u.ProyectosCreados)
            .FirstOrDefaultAsync(u => u.Id == usuarioId, cancellationToken);
    }

    public async Task<Usuario?> ObtenerUsuarioPorEmailAsync(string emailNormalizado, CancellationToken cancellationToken = default)
    {
        return await context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == emailNormalizado, cancellationToken);
    }

    public void EliminarUsuario(Usuario usuario) => context.Usuarios.Remove(usuario);
}
