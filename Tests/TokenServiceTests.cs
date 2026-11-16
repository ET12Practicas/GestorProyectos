using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dominio.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Presentacion.Servicios;
using Xunit;

namespace Tests;

public class TokenServiceTests
{
    private static IConfiguration ConfiguracionJwt() => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:ClaveSecreta"] = "ClaveDePruebaParaTestsConMinimo32Caracteres!",
            ["Jwt:Emisor"] = "GestorProyectosApi",
            ["Jwt:Audiencia"] = "GestorProyectosClient",
            ["Jwt:ExpiracionMinutos"] = "120"
        })
        .Build();

    private static UserManager<Usuario> CrearUserManager(Mock<IUserRoleStore<Usuario>> store) => new(
        store.Object,
        Options.Create(new IdentityOptions()),
        new PasswordHasher<Usuario>(),
        [],
        [],
        new UpperInvariantLookupNormalizer(),
        new IdentityErrorDescriber(),
        null!,
        NullLogger<UserManager<Usuario>>.Instance);

    private static JwtSecurityToken LeerToken(string token)
    {
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        return handler.ReadJwtToken(token);
    }

    [Fact]
    public async Task GenerarToken_ConRolAdministrador_IncluyeClaimDeRol()
    {
        var store = new Mock<IUserRoleStore<Usuario>>();
        store.Setup(s => s.GetRolesAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { "Administrador" });

        var tokenService = new TokenService(ConfiguracionJwt(), CrearUserManager(store));
        var usuario = new Usuario("Admin") { Email = "admin@demo.com" };

        var (token, expiraEn) = await tokenService.GenerarTokenAsync(usuario);

        var jwt = LeerToken(token);
        Assert.Contains(jwt.Claims, c => c.Type == "role" && c.Value == "Administrador");
        Assert.Contains(jwt.Claims, c => c.Type == "nameid" && c.Value == usuario.Id.ToString());
        Assert.True(expiraEn > DateTime.UtcNow);
    }

    [Fact]
    public async Task GenerarToken_SinRoles_NoIncluyeClaimDeRol()
    {
        var store = new Mock<IUserRoleStore<Usuario>>();
        store.Setup(s => s.GetRolesAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());

        var tokenService = new TokenService(ConfiguracionJwt(), CrearUserManager(store));
        var usuario = new Usuario("Usuario") { Email = "usuario@demo.com" };

        var (token, _) = await tokenService.GenerarTokenAsync(usuario);

        var jwt = LeerToken(token);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == "role");
    }
}
