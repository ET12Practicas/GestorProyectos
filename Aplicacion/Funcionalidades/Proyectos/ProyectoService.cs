using Aplicacion.Abstracciones;
using Aplicacion.Comun;
using Aplicacion.Dominio;
using Aplicacion.Funcionalidades.Tickets;
using Aplicacion.Funcionalidades.Usuarios;
using FluentValidation;

namespace Aplicacion.Funcionalidades.Proyectos;

public interface IProyectoService
{
    Task<IReadOnlyList<ProyectoQueryDto>> ObtenerProyectosAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProyectoQueryDto>> ObtenerProyectosDelUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<ProyectoQueryDto?> ObtenerProyectoPorIdAsync(Guid proyectoId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CrearProyectoAsync(ProyectoCommandDto dto, CancellationToken cancellationToken = default);
    Task<Result> AsignarUsuarioAProyectoAsync(Guid proyectoId, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Result> QuitarUsuarioDeProyectoAsync(Guid proyectoId, Guid usuarioId, CancellationToken cancellationToken = default);
    Task<Result> EliminarProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default);
}

internal sealed class ProyectoService(
    IProyectoRepository proyectoRepository,
    IUsuarioRepository usuarioRepository,
    IUnidadDeTrabajo unidadDeTrabajo,
    IValidator<ProyectoCommandDto> validator) : IProyectoService
{
    public async Task<IReadOnlyList<ProyectoQueryDto>> ObtenerProyectosAsync(CancellationToken cancellationToken = default)
    {
        var proyectos = await proyectoRepository.ObtenerProyectosConDetallesAsync(cancellationToken);
        return proyectos.Select(MapearProyecto).ToList();
    }

    public async Task<IReadOnlyList<ProyectoQueryDto>> ObtenerProyectosDelUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var proyectos = await proyectoRepository.ObtenerProyectosDelUsuarioAsync(usuarioId, cancellationToken);
        return proyectos.Select(MapearProyecto).ToList();
    }

    public async Task<ProyectoQueryDto?> ObtenerProyectoPorIdAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        return proyecto is null ? null : MapearProyecto(proyecto);
    }

    public async Task<Result<Guid>> CrearProyectoAsync(ProyectoCommandDto dto, CancellationToken cancellationToken = default)
    {
        var validacion = await validator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
        {
            var primerError = validacion.Errors[0].ErrorMessage;
            return Result.Failure<Guid>(Error.Validation(primerError));
        }

        var proyecto = new Proyecto(dto.Nombre.Trim(), dto.CreadorId);
        if (dto.CreadorId.HasValue)
        {
            var creador = await usuarioRepository.ObtenerUsuarioAsync(dto.CreadorId.Value, cancellationToken);
            if (creador is not null)
            {
                proyecto.Usuarios.Add(creador);
            }
        }

        proyectoRepository.AgregarProyecto(proyecto);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Success(proyecto.Id);
    }

    public async Task<Result> AsignarUsuarioAProyectoAsync(Guid proyectoId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        if (proyecto is null)
        {
            return Result.Failure(Error.NotFound("Proyecto no encontrado."));
        }

        var usuario = await usuarioRepository.ObtenerUsuarioAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure(Error.NotFound("Usuario no encontrado."));
        }

        if (proyecto.Usuarios.Any(u => u.Id == usuarioId))
        {
            return Result.Success(); // Ya pertenece al proyecto
        }

        proyecto.Usuarios.Add(usuario);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> QuitarUsuarioDeProyectoAsync(Guid proyectoId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        if (proyecto is null)
        {
            return Result.Failure(Error.NotFound("Proyecto no encontrado."));
        }

        var usuario = proyecto.Usuarios.FirstOrDefault(u => u.Id == usuarioId);
        if (usuario is null)
        {
            return Result.Failure(Error.NotFound("El usuario no forma parte de este proyecto."));
        }

        // Si el usuario tenía tickets asignados en este proyecto, se desasignan
        foreach (var ticket in proyecto.Tickets.Where(t => t.UsuarioId == usuarioId))
        {
            ticket.UsuarioTicket = null;
            ticket.UsuarioId = null;
        }

        proyecto.Usuarios.Remove(usuario);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result> EliminarProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        if (proyecto is null)
        {
            return Result.Failure(Error.NotFound("Proyecto no encontrado."));
        }

        proyectoRepository.EliminarProyecto(proyecto);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    internal static ProyectoQueryDto MapearProyecto(Proyecto proyecto)
    {
        return new ProyectoQueryDto
        {
            Id = proyecto.Id,
            Nombre = proyecto.Nombre,
            CreadorId = proyecto.CreadorId,
            Creador = proyecto.Creador is null ? null : UsuarioService.MapearUsuario(proyecto.Creador),
            Tickets = proyecto.Tickets.Select(TicketService.MapearTicket).ToList(),
            Usuarios = proyecto.Usuarios.Select(UsuarioService.MapearUsuario).ToList()
        };
    }
}
