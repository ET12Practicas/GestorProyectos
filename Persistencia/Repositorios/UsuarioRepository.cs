using Aplicacion.Abstracciones;
using Aplicacion.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Persistencia;

internal sealed class UsuarioRepository(ProyectoDbContext context) : IUsuarioRepository
{
    public IReadOnlyList<Usuario> ObtenerUsuarios()
    {
        return context.Usuarios.AsNoTracking().ToList();
    }

    public Usuario? ObtenerUsuario(Guid usuarioId)
    {
        return context.Usuarios.FirstOrDefault(usuario => usuario.Id == usuarioId);
    }

    public void AgregarUsuario(Usuario usuario) => context.Usuarios.Add(usuario);

    public void EliminarUsuario(Usuario usuario) => context.Usuarios.Remove(usuario);
}
