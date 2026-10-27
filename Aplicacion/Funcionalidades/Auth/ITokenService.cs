using Aplicacion.Dominio;

namespace Aplicacion.Funcionalidades.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
}
