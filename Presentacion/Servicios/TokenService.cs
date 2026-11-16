using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dominio.Entidades;
using Aplicacion.Funcionalidades.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Presentacion.Servicios;

public sealed class TokenService(IConfiguration configuration, UserManager<Usuario> userManager) : ITokenService
{
    public async Task<(string Token, DateTime ExpiraEn)> GenerarTokenAsync(Usuario usuario)
    {
        var roles = await userManager.GetRolesAsync(usuario);

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

        claims.AddRange(roles.Select(rol => new Claim(ClaimTypes.Role, rol)));

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
