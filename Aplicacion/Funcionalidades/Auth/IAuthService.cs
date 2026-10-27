using Aplicacion.Comun;

namespace Aplicacion.Funcionalidades.Auth;

public interface IAuthService
{
    Task<Result<UsuarioAutenticadoDto>> RegistrarAsync(RegistroUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<Result<UsuarioAutenticadoDto>> IniciarSesionAsync(LoginUsuarioDto dto, CancellationToken cancellationToken = default);
    Task CerrarSesionAsync();
}
