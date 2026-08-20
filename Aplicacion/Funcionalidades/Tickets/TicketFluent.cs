using FluentValidation;
using Aplicacion.Dominio;

namespace Aplicacion.Funcionalidades.Tickets
{
    public class TicketFluent : AbstractValidator<Ticket>
    {
        public TicketFluent()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id no detectado.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del ticket no puede estar vacio")
                .MaximumLength(45).WithMessage("El nombre del ticket no puede superar los 45 caracteres.");

            RuleFor(x => x.Descripcion)
                .NotEmpty().WithMessage("El contenido del ticket no puede estar vacio");

            RuleFor(x => x.FechaTicket)
                .NotEmpty().WithMessage("Fecha del ticket es necesario");
        }
    }
}