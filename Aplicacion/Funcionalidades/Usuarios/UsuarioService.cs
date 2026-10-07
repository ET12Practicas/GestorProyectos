using Aplicacion.Abstracciones;
using Aplicacion.Comun;
using Aplicacion.Dominio;
using FluentValidation;

namespace Aplicacion.Funcionalidades.Usuarios;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioQueryDto>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default);
    Task<UsuarioQueryDto?> ObtenerUsuarioPorIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CrearUsuarioAsync(UsuarioCommandDto dto, CancellationToken cancellationToken = default);
    Task<Result> EliminarUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}

internal sealed class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IUnidadDeTrabajo unidadDeTrabajo,
    IValidator<UsuarioCommandDto> validator) : IUsuarioService
{
    public async Task<IReadOnlyList<UsuarioQueryDto>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await usuarioRepository.ObtenerUsuariosAsync(cancellationToken);
        return usuarios.Select(MapearUsuario).ToList();
    }

    public async Task<UsuarioQueryDto?> ObtenerUsuarioPorIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerUsuarioAsync(usuarioId, cancellationToken);
        return usuario is null ? null : MapearUsuario(usuario);
    }

    public async Task<Result<Guid>> CrearUsuarioAsync(UsuarioCommandDto dto, CancellationToken cancellationToken = default)
    {
        var validacion = await validator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
        {
            var primerError = validacion.Errors[0].ErrorMessage;
            return Result.Failure<Guid>(Error.Validation(primerError));
        }

        var usuario = new Usuario(dto.Nombre.Trim());
        usuarioRepository.AgregarUsuario(usuario);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Success(usuario.Id);
    }

    public async Task<Result> EliminarUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerUsuarioAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure(Error.NotFound("Usuario no encontrado."));
        }

        usuarioRepository.EliminarUsuario(usuario);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    internal static UsuarioQueryDto MapearUsuario(Usuario usuario)
    {
        return new UsuarioQueryDto { Id = usuario.Id, Nombre = usuario.Nombre };
    }
}
