
using FluentValidation;
using Aplicacion.Dominio;

namespace Aplicacion.Funcionalidades.Comentarios
{
    public class ComentarioFluent : AbstractValidator<Comentario>
    {
        public ComentarioFluent()
        {
            RuleFor(x => x.IdComentario)
                .NotEmpty()
                .WithMessage("Id no detectado.");

            RuleFor(x => x.FechaComentario)
                .NotEmpty()
                .WithMessage("Fecha no ingresada.");

            RuleFor(x => x.Contenido)
                .NotEmpty()
                .WithMessage("El comentario no puede estar vacio.");
        }
    }
}