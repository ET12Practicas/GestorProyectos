using Aplicacion.Abstracciones;
using Aplicacion.Comun;
using Dominio.Entidades;
using FluentValidation;

namespace Aplicacion.Funcionalidades.Usuarios;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioQueryDto>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default);
    Task<UsuarioQueryDto?> ObtenerUsuarioPorIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);

    Task<Result> EliminarUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
}

internal sealed class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IProyectoRepository proyectoRepository,
    ITicketRepository ticketRepository,
    IUnidadDeTrabajo unidadDeTrabajo) : IUsuarioService
{
    public async Task<IReadOnlyList<UsuarioQueryDto>> ObtenerUsuariosAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await usuarioRepository.ObtenerUsuariosAsync(cancellationToken);
        return usuarios.Select(MapearUsuario).ToList();
    }

    public async Task<UsuarioQueryDto?> ObtenerUsuarioPorIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerUsuarioAsync(usuarioId, cancellationToken);
        return usuario is null ? null : MapearUsuario(usuario);
    }

    public async Task<Result> EliminarUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await usuarioRepository.ObtenerUsuarioConRelacionesAsync(usuarioId, cancellationToken);
        if (usuario is null)
        {
            return Result.Failure(Error.NotFound("Usuario no encontrado."));
        }

        var proyectosDelUsuario = await proyectoRepository.ObtenerProyectosDelUsuarioAsync(usuarioId, cancellationToken);
        foreach (var proyecto in proyectosDelUsuario)
        {
            if (proyecto.CreadorId == usuarioId)
            {
                proyecto.CreadorId = null;
                proyecto.Creador = null;
            }

            proyecto.Usuarios.RemoveAll(u => u.Id == usuarioId);

            foreach (var ticket in proyecto.Tickets.Where(t => t.UsuarioId == usuarioId))
            {
                ticket.UsuarioTicket = null;
                ticket.UsuarioId = null;
            }
        }

        var ticketsDelUsuario = await ticketRepository.ObtenerTicketsDelUsuarioAsync(usuarioId, cancellationToken);
        foreach (var ticket in ticketsDelUsuario)
        {
            ticket.UsuarioTicket = null;
            ticket.UsuarioId = null;
        }

        usuarioRepository.EliminarUsuario(usuario);
        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        return Result.Success();
    }

    internal static UsuarioQueryDto MapearUsuario(Usuario usuario)
    {
        return new UsuarioQueryDto { Id = usuario.Id, Nombre = usuario.Nombre };
    }
}
