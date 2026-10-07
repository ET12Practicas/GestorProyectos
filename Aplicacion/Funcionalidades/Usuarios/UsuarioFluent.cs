using FluentValidation;

namespace Aplicacion.Funcionalidades.Usuarios;

public class UsuarioCommandDtoValidator : AbstractValidator<UsuarioCommandDto>
{
    public UsuarioCommandDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MaximumLength(45).WithMessage("El nombre no puede superar los 45 caracteres.");
    }
}