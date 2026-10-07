using FluentValidation;

namespace Aplicacion.Funcionalidades.Proyectos;

public class ProyectoCommandDtoValidator : AbstractValidator<ProyectoCommandDto>
{
    public ProyectoCommandDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del proyecto es obligatorio.")
            .MaximumLength(45).WithMessage("El nombre del proyecto no debe superar los 45 caracteres.");
    }
}