using System.Security.Claims;
using Aplicacion.Comun;
using Aplicacion.Dominio;
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
            UserManager<Usuario> userManager,
            IValidator<RegistroUsuarioDto> validator,
            RegistroUsuarioDto dto,
            CancellationToken ct) =>
        {
            var validacion = await validator.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
            {
                var primerError = validacion.Errors[0].ErrorMessage;
                return Results.BadRequest(new { error = primerError });
            }

            var existeUsuario = await userManager.FindByEmailAsync(dto.Email);
            if (existeUsuario is not null)
            {
                return Results.BadRequest(new { error = "Ya existe un usuario registrado con ese email." });
            }

            var usuario = new Usuario(dto.Nombre.Trim())
            {
                UserName = dto.Email.Trim(),
                Email = dto.Email.Trim(),
                EmailConfirmed = true
            };

            var resultado = await userManager.CreateAsync(usuario, dto.Password);
            if (!resultado.Succeeded)
            {
                var mensajeError = string.Join(", ", resultado.Errors.Select(e => e.Description));
                return Results.BadRequest(new { error = mensajeError });
            }

            var respuesta = new UsuarioAutenticadoDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email!
            };

            return Results.Created($"/api/usuarios/{usuario.Id}", respuesta);
        });

        group.MapPost("/login", async (
            SignInManager<Usuario> signInManager,
            UserManager<Usuario> userManager,
            IValidator<LoginUsuarioDto> validator,
            LoginUsuarioDto dto,
            CancellationToken ct) =>
        {
            var validacion = await validator.ValidateAsync(dto, ct);
            if (!validacion.IsValid)
            {
                var primerError = validacion.Errors[0].ErrorMessage;
                return Results.BadRequest(new { error = primerError });
            }

            var usuario = await userManager.FindByEmailAsync(dto.Email.Trim());
            if (usuario is null)
            {
                return Results.Unauthorized();
            }

            var resultado = await signInManager.PasswordSignInAsync(
                usuario,
                dto.Password,
                isPersistent: dto.Recordarme,
                lockoutOnFailure: false);

            if (!resultado.Succeeded)
            {
                return Results.Unauthorized();
            }

            var respuesta = new UsuarioAutenticadoDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email ?? dto.Email
            };

            return Results.Ok(respuesta);
        });

        group.MapPost("/logout", async (SignInManager<Usuario> signInManager) =>
        {
            await signInManager.SignOutAsync();
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
