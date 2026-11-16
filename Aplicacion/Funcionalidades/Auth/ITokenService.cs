using Dominio.Entidades;

namespace Aplicacion.Funcionalidades.Auth;

public interface ITokenService
{
    Task<(string Token, DateTime ExpiraEn)> GenerarTokenAsync(Usuario usuario);
}
