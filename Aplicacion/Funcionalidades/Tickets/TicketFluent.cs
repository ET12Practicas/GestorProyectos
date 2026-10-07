using FluentValidation;

namespace Aplicacion.Funcionalidades.Tickets;

public class TicketCommandDtoValidator : AbstractValidator<TicketCommandDto>
{
    public TicketCommandDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del ticket es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre del ticket no puede superar los 100 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción del ticket no puede superar los 1000 caracteres.");
    }
}