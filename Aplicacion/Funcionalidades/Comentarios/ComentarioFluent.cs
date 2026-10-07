using FluentValidation;

namespace Aplicacion.Funcionalidades.Comentarios;

public class ComentarioCommandDtoValidator : AbstractValidator<ComentarioCommandDto>
{
    public ComentarioCommandDtoValidator()
    {
        RuleFor(x => x.Contenido)
            .NotEmpty().WithMessage("El comentario no puede estar vacío.")
            .MaximumLength(2000).WithMessage("El comentario no puede superar los 2000 caracteres.");
    }
}