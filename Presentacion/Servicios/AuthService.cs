using Aplicacion.Comun;
using Aplicacion.Dominio;
using Aplicacion.Funcionalidades.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Presentacion.Servicios;

public sealed class AuthService(
    UserManager<Usuario> userManager,
    SignInManager<Usuario> signInManager,
    ITokenService tokenService,
    IValidator<RegistroUsuarioDto> registroValidator,
    IValidator<LoginUsuarioDto> loginValidator) : IAuthService
{
    public async Task<Result<UsuarioAutenticadoDto>> RegistrarAsync(
        RegistroUsuarioDto dto,
        CancellationToken cancellationToken = default)
    {
        var validacion = await registroValidator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
        {
            var primerError = validacion.Errors[0].ErrorMessage;
            return Result.Failure<UsuarioAutenticadoDto>(Error.Validation(primerError));
        }

        var existeUsuario = await userManager.FindByEmailAsync(dto.Email.Trim());
        if (existeUsuario is not null)
        {
            return Result.Failure<UsuarioAutenticadoDto>(Error.Conflict("Ya existe un usuario registrado con ese email."));
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
            return Result.Failure<UsuarioAutenticadoDto>(Error.Validation(mensajeError));
        }

        // Emitir cookie de sesión
        await signInManager.SignInAsync(usuario, isPersistent: false);

        // Emitir token JWT
        var (token, expiraEn) = tokenService.GenerarToken(usuario);

        return Result.Success(new UsuarioAutenticadoDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email!,
            Token = token,
            ExpiraEn = expiraEn
        });
    }

    public async Task<Result<UsuarioAutenticadoDto>> IniciarSesionAsync(
        LoginUsuarioDto dto,
        CancellationToken cancellationToken = default)
    {
        var validacion = await loginValidator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
        {
            var primerError = validacion.Errors[0].ErrorMessage;
            return Result.Failure<UsuarioAutenticadoDto>(Error.Validation(primerError));
        }

        var usuario = await userManager.FindByEmailAsync(dto.Email.Trim());
        if (usuario is null)
        {
            return Result.Failure<UsuarioAutenticadoDto>(Error.Unauthorized("Credenciales inválidas."));
        }

        // Valida password y emite cookie de sesión en la respuesta HTTP
        var resultado = await signInManager.PasswordSignInAsync(
            usuario,
            dto.Password,
            isPersistent: dto.Recordarme,
            lockoutOnFailure: false);

        if (!resultado.Succeeded)
        {
            return Result.Failure<UsuarioAutenticadoDto>(Error.Unauthorized("Credenciales inválidas."));
        }

        // Emitir token JWT
        var (token, expiraEn) = tokenService.GenerarToken(usuario);

        return Result.Success(new UsuarioAutenticadoDto
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Email = usuario.Email ?? dto.Email,
            Token = token,
            ExpiraEn = expiraEn
        });
    }

    public async Task CerrarSesionAsync()
    {
        await signInManager.SignOutAsync();
    }
}
