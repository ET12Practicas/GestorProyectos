using Aplicacion.Abstracciones;
using Aplicacion.Comun;
using Aplicacion.Dominio;
using FluentValidation;

namespace Aplicacion.Funcionalidades.Comentarios;

public interface IComentarioService
{
    Task<IReadOnlyList<ComentarioQueryDto>> ObtenerComentariosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ComentarioQueryDto>> ObtenerComentariosPorTicketAsync(Guid ticketId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CrearComentarioAsync(Guid ticketId, Guid usuarioId, ComentarioCommandDto dto, CancellationToken cancellationToken = default);
    Task<Result> EliminarComentarioAsync(Guid comentarioId, CancellationToken cancellationToken = default);
}

internal sealed class ComentarioService(
    IComentarioRepository comentarioRepository,
    ITicketRepository ticketRepository,
    IUsuarioRepository usuarioRepository,
    IProyectoRepository proyectoRepository,
    IUnidadDeTrabajo unidadDeTrabajo,
    IValidator<ComentarioCommandDto> validator) : IComentarioService
{
    public async Task<IReadOnlyList<ComentarioQueryDto>> ObtenerComentariosAsync(CancellationToken cancellationToken = default)
    {
        var comentarios = await comentarioRepository.ObtenerComentariosConDetallesAsync(cancellationToken);
        return comentarios.Select(MapearComentario).ToList();
    }

    public async Task<IReadOnlyList<ComentarioQueryDto>> ObtenerComentariosPorTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        var comentarios = await comentarioRepository.ObtenerComentariosPorTicketAsync(ticketId, cancellationToken);
        return comentarios.Select(MapearComentario).ToList();
    }

    public async Task<Result<Guid>> CrearComentarioAsync(
        Guid ticketId,
        Guid usuarioId,
        ComentarioCommandDto dto,
        CancellationToken cancellationToken = default)
    {
        var validacion = await validator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
        {
            var primerError = validacion.Errors[0].ErrorMessage;
            return Result.Failure<Guid>(Error.Validation(primerError));
        }

        var ticket = await ticketRepository.ObtenerTicketAsync(ticketId, cancellationToken);
        if (ticket is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Ticket no encontrado."));
        }

        var usuario = await usuarioRepository.ObtenerUsuarioAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure<Guid>(Error.NotFound("Usuario no encontrado."));
        }

        var perteneceAlProyecto = await proyectoRepository.ExisteProyectoConUsuarioYTicketAsync(usuarioId, ticketId, cancellationToken);
        if (!perteneceAlProyecto)
        {
            return Result.Failure<Guid>(Error.Conflict("El usuario debe pertenecer al proyecto para comentar este ticket."));
        }

        var comentario = new Comentario(dto.Contenido.Trim())
        {
            UsuarioId = usuario.Id,
            UsuarioComentario = usuario,
            TicketId = ticket.Id,
            Ticket = ticket
        };

        ticket.AgregarComentario(comentario);
        comentarioRepository.AgregarComentario(comentario);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Success(comentario.Id);
    }

    public async Task<Result> EliminarComentarioAsync(Guid comentarioId, CancellationToken cancellationToken = default)
    {
        var comentario = await comentarioRepository.ObtenerComentarioAsync(comentarioId, cancellationToken);
        if (comentario is null)
        {
            return Result.Failure(Error.NotFound("Comentario no encontrado."));
        }

        comentarioRepository.EliminarComentario(comentario);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    internal static ComentarioQueryDto MapearComentario(Comentario comentario)
    {
        return new ComentarioQueryDto
        {
            Id = comentario.Id,
            Contenido = comentario.Contenido,
            Fecha = comentario.FechaComentario,
            Usuario = comentario.UsuarioComentario is null
                ? null
                : Usuarios.UsuarioService.MapearUsuario(comentario.UsuarioComentario)
        };
    }
}
