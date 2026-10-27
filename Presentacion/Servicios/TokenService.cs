using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Aplicacion.Dominio;
using Aplicacion.Funcionalidades.Auth;
using Microsoft.IdentityModel.Tokens;

namespace Presentacion.Servicios;

public sealed class TokenService(IConfiguration configuration) : ITokenService
{
    public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var claveSecreta = jwtSettings["ClaveSecreta"]
            ?? throw new InvalidOperationException("No se configuró 'Jwt:ClaveSecreta' en appsettings.json.");

        var emisor = jwtSettings["Emisor"] ?? "GestorProyectosApi";
        var audiencia = jwtSettings["Audiencia"] ?? "GestorProyectosClient";
        var minutosStr = jwtSettings["ExpiracionMinutos"] ?? "120";
        var expiracionMinutos = int.TryParse(minutosStr, out var m) ? m : 120;

        var expiraEn = DateTime.UtcNow.AddMinutes(expiracionMinutos);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Email ?? usuario.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(claveSecreta));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiraEn,
            Issuer = emisor,
            Audience = audiencia,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(securityToken);

        return (tokenString, expiraEn);
    }
}
