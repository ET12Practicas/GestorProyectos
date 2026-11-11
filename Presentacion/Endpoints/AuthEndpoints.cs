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
            return result.IsSuccess 
                ? Results.Ok(new { mensaje = result.Value }) 
                : result.ToHttpResult();
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

        // Endpoint para procesar el Login desde el formulario Blazor con Cookie
        app.MapPost("/auth/login-cookie", async (
            HttpContext context,
            SignInManager<Usuario> signInManager,
            UserManager<Usuario> userManager) =>
        {
            var form = await context.Request.ReadFormAsync();
            var email = form["email"].ToString().Trim();
            var password = form["password"].ToString();
            var recordarme = form["recordarme"].ToString() == "true";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Results.Redirect("/login?error=datos");
            }

            var usuario = await userManager.FindByEmailAsync(email);
            if (usuario is null)
            {
                return Results.Redirect("/login?error=credenciales");
            }

            var result = await signInManager.PasswordSignInAsync(
                usuario,
                password,
                isPersistent: recordarme,
                lockoutOnFailure: false);

            if (!result.Succeeded)
            {
                return Results.Redirect("/login?error=credenciales");
            }

            return Results.Redirect("/");
        }).DisableAntiforgery();

        // Endpoint para procesar el Registro desde el formulario Blazor con Cookie
        app.MapPost("/auth/registro-cookie", async (
            HttpContext context,
            IAuthService authService) =>
        {
            var form = await context.Request.ReadFormAsync();
            var nombre = form["nombre"].ToString().Trim();
            var email = form["email"].ToString().Trim();
            var password = form["password"].ToString();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return Results.Redirect("/registro?error=datos");
            }

            var dto = new RegistroUsuarioDto
            {
                Nombre = nombre,
                Email = email,
                Password = password
            };

            var result = await authService.RegistrarAsync(dto);
            if (result.IsFailure)
            {
                var errorParam = result.Error.Codigo == "Conflict"
                    ? "duplicado"
                    : Uri.EscapeDataString(result.Error.Mensaje);

                return Results.Redirect($"/registro?error={errorParam}");
            }

            // Al registrarse exitosamente, lo redirigimos a login
            return Results.Redirect("/login");
        }).DisableAntiforgery();

        // Endpoint para cerrar sesión desde el navegador (form POST directo)
        app.MapPost("/auth/logout", async (IAuthService authService) =>
        {
            await authService.CerrarSesionAsync();
            return Results.Redirect("/login");
        });

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
