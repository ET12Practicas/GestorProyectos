using FluentValidation;
using Aplicacion.Dominio;

namespace Aplicacion.Funcionalidades.Usuarios
{
    public class UsuarioFluent : AbstractValidator<Usuario>
    {
        public UsuarioFluent()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id no detectado.");

            RuleFor (x => x.Nombre)
                .NotEmpty().WithMessage("No se puede dejar vacio este espacio.")
                .MaximumLength(45).WithMessage("El nombre no puede superar los 45 caracteres");
        }
        
    }
}