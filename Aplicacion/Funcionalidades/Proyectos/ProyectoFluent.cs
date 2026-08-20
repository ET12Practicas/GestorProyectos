using FluentValidation;
using Aplicacion.Dominio;

namespace Aplicacion.Funcionalidades.Proyectos
{
    public class ProyectoFluent : AbstractValidator<Proyecto>
    {
        public ProyectoFluent()
        {
            RuleFor(x => x.IdProject)
                .NotEmpty().WithMessage("Id no detectado.");
            
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El proyecto debe tener un nombre")
                .MaximumLength(45).WithMessage("El nombre del proyecto no debe pasar los 45 caracteres.");
        }
    }
}