using Dominio.Entidades;

namespace Aplicacion.Funcionalidades.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
}
