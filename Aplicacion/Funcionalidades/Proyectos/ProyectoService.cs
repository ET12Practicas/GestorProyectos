using Aplicacion.Abstracciones;
using Aplicacion.Comun;
using Dominio.Entidades;
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
    IValidator<ProyectoCommandDto> validator,
    ICurrentUserService currentUserService) : IProyectoService
{
    public async Task<IReadOnlyList<ProyectoQueryDto>> ObtenerProyectosAsync(CancellationToken cancellationToken = default)
    {
        if (!currentUserService.EsAdministrador)
        {
            return [];
        }

        var proyectos = await proyectoRepository.ObtenerProyectosConDetallesAsync(cancellationToken);
        return proyectos.Select(MapearProyecto).ToList();
    }

    public async Task<IReadOnlyList<ProyectoQueryDto>> ObtenerProyectosDelUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        if (!PuedeVerUsuario(usuarioId))
        {
            return [];
        }

        var proyectos = await proyectoRepository.ObtenerProyectosDelUsuarioAsync(usuarioId, cancellationToken);
        return proyectos.Select(MapearProyecto).ToList();
    }

    public async Task<ProyectoQueryDto?> ObtenerProyectoPorIdAsync(Guid proyectoId, CancellationToken cancellationToken = default)
    {
        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        if (proyecto is null || !TieneAccesoAlProyecto(proyecto))
        {
            return null;
        }

        return MapearProyecto(proyecto);
    }

    public async Task<Result<Guid>> CrearProyectoAsync(ProyectoCommandDto dto, CancellationToken cancellationToken = default)
    {
        var validacion = await validator.ValidateAsync(dto, cancellationToken);
        if (!validacion.IsValid)
        {
            var primerError = validacion.Errors[0].ErrorMessage;
            return Result.Failure<Guid>(Error.Validation(primerError));
        }

        var usuarioActualId = currentUserService.UsuarioId;
        if (!usuarioActualId.HasValue)
        {
            return Result.Failure<Guid>(Error.Unauthorized("Debes iniciar sesión para crear un proyecto."));
        }

        if (dto.CreadorId.HasValue && dto.CreadorId.Value != usuarioActualId.Value)
        {
            return Result.Failure<Guid>(Error.Forbidden("No puedes crear un proyecto para otro usuario."));
        }

        var proyecto = new Proyecto(dto.Nombre.Trim(), usuarioActualId.Value);
        var creador = await usuarioRepository.ObtenerUsuarioAsync(usuarioActualId.Value, cancellationToken);
        if (creador is not null)
        {
            proyecto.Usuarios.Add(creador);
        }

        proyectoRepository.AgregarProyecto(proyecto);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return Result.Success(proyecto.Id);
    }

    public async Task<Result> AsignarUsuarioAProyectoAsync(Guid proyectoId, Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuarioActualId = currentUserService.UsuarioId;
        if (!usuarioActualId.HasValue)
        {
            return Result.Failure(Error.Unauthorized("Debes iniciar sesión."));
        }

        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        if (proyecto is null)
        {
            return Result.Failure(Error.NotFound("Proyecto no encontrado."));
        }

        // Regla: Solo el creador del proyecto puede asignar nuevos usuarios
        if (proyecto.CreadorId != usuarioActualId)
        {
            return Result.Failure(Error.Forbidden("Solo el creador del proyecto puede asignar usuarios a este proyecto."));
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
        var usuarioActualId = currentUserService.UsuarioId;
        if (!usuarioActualId.HasValue)
        {
            return Result.Failure(Error.Unauthorized("Debes iniciar sesión."));
        }

        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        if (proyecto is null)
        {
            return Result.Failure(Error.NotFound("Proyecto no encontrado."));
        }

        // Regla: Solo el creador del proyecto puede quitar usuarios (o el mismo usuario saliendo)
        if (proyecto.CreadorId != usuarioActualId && usuarioActualId != usuarioId)
        {
            return Result.Failure(Error.Forbidden("Solo el creador del proyecto puede quitar colaboradores."));
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
        var usuarioActualId = currentUserService.UsuarioId;
        if (!usuarioActualId.HasValue)
        {
            return Result.Failure(Error.Unauthorized("Debes iniciar sesión."));
        }

        var proyecto = await proyectoRepository.ObtenerProyectoAsync(proyectoId, cancellationToken);
        if (proyecto is null)
        {
            return Result.Failure(Error.NotFound("Proyecto no encontrado."));
        }

        // Regla: Solo el creador del proyecto puede eliminarlo
        if (proyecto.CreadorId != usuarioActualId)
        {
            return Result.Failure(Error.Forbidden("Solo el creador del proyecto puede eliminarlo."));
        }

        proyectoRepository.EliminarProyecto(proyecto);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    private bool PuedeVerUsuario(Guid usuarioId)
    {
        var actualId = currentUserService.UsuarioId;
        if (!actualId.HasValue) return false;
        return actualId.Value == usuarioId || currentUserService.EsAdministrador;
    }

    private bool TieneAccesoAlProyecto(Proyecto proyecto)
    {
        var actualId = currentUserService.UsuarioId;
        if (!actualId.HasValue) return false;
        if (currentUserService.EsAdministrador) return true;
        return proyecto.CreadorId == actualId.Value || proyecto.Usuarios.Any(u => u.Id == actualId.Value);
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
