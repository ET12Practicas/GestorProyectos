using System.Security.Claims;
using Aplicacion.Comun;
using Dominio.Entidades;
using Aplicacion.Funcionalidades.Auth;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Presentacion.Endpoints;

public sealed class AuthEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Autenticación");

        group.MapPost("/registro", async (
            IAuthService authService,
            RegistroUsuarioDto dto,
            CancellationToken ct) =>
        {
            var result = await authService.RegistrarAsync(dto, ct);
            return result.ToHttpResult($"/api/usuarios/{result.Value?.Id}");
        });

        group.MapPost("/login", async (
            IAuthService authService,
            LoginUsuarioDto dto,
            CancellationToken ct) =>
        {
            var result = await authService.IniciarSesionAsync(dto, ct);
            return result.ToHttpResult();
        });

        group.MapPost("/logout", async (IAuthService authService) =>
        {
            await authService.CerrarSesionAsync();
            return Results.Ok(new { mensaje = "Sesión cerrada correctamente." });
        }).RequireAuthorization();

        group.MapGet("/me", async (
            ClaimsPrincipal principal,
            UserManager<Usuario> userManager) =>
        {
            if (principal.Identity is null || !principal.Identity.IsAuthenticated)
            {
                return Results.Unauthorized();
            }

            var usuario = await userManager.GetUserAsync(principal);
            if (usuario is null)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new UsuarioAutenticadoDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email ?? string.Empty
            });
        }).RequireAuthorization();
    }
}
