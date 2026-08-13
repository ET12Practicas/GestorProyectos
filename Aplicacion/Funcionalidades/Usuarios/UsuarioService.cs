using Aplicacion.Abstracciones;
using Aplicacion.Dominio;

namespace Aplicacion.Funcionalidades.Usuarios;

public interface IUsuarioService
{
    IReadOnlyList<UsuarioQueryDto> ObtenerUsuarios();
    bool CrearUsuario(UsuarioCommandDto dto);
    bool EliminarUsuario(Guid usuarioId);
}

internal sealed class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IUnidadDeTrabajo unidadDeTrabajo) : IUsuarioService
{
    public IReadOnlyList<UsuarioQueryDto> ObtenerUsuarios()
    {
        return usuarioRepository.ObtenerUsuarios().Select(MapearUsuario).ToList();
    }

    public bool CrearUsuario(UsuarioCommandDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
        {
            return false;
        }

        usuarioRepository.AgregarUsuario(new Usuario(dto.Nombre.Trim()));
        unidadDeTrabajo.GuardarCambios();
        return true;
    }

    public bool EliminarUsuario(Guid usuarioId)
    {
        var usuario = usuarioRepository.ObtenerUsuario(usuarioId);
        if (usuario is null)
        {
            return false;
        }

        usuarioRepository.EliminarUsuario(usuario);
        unidadDeTrabajo.GuardarCambios();
        return true;
    }

    internal static UsuarioQueryDto MapearUsuario(Usuario usuario)
    {
        return new UsuarioQueryDto { Id = usuario.Id, Nombre = usuario.Nombre };
    }
}
