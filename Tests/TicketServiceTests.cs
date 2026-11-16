using Aplicacion.Funcionalidades.Tickets;
using Dominio.Entidades;
using Tests.Fakes;
using Xunit;

namespace Tests;

public class TicketServiceTests
{
    private readonly FakeTicketRepository _ticketRepository = new();
    private readonly FakeProyectoRepository _proyectoRepository = new();
    private readonly FakeUsuarioRepository _usuarioRepository = new();
    private readonly FakeUnidadDeTrabajo _unidadDeTrabajo = new();
    private readonly FakeCurrentUserService _currentUser = new();

    private TicketService CrearService() => new(
        _ticketRepository,
        _proyectoRepository,
        _usuarioRepository,
        _unidadDeTrabajo,
        new TicketCommandDtoValidator(),
        _currentUser);

    private (Proyecto proyecto, Ticket ticket) SembrarProyectoConTicket(Guid creadorId)
    {
        var proyecto = new Proyecto("Proyecto", creadorId);
        var ticket = new Ticket("Ticket")
        {
            ProyectoId = proyecto.Id,
            ProyectoTicket = proyecto
        };
        proyecto.Tickets.Add(ticket);
        _proyectoRepository.Proyectos.Add(proyecto);
        _ticketRepository.Tickets.Add(ticket);
        return (proyecto, ticket);
    }

    [Fact]
    public async Task EliminarTicket_SinSesion_DevuelveUnauthorized()
    {
        var (_, ticket) = SembrarProyectoConTicket(Guid.NewGuid());
        _currentUser.UsuarioId = null;

        var resultado = await CrearService().EliminarTicketAsync(ticket.Id);

        Assert.True(resultado.IsFailure);
        Assert.Equal("Unauthorized", resultado.Error.Codigo);
    }

    [Fact]
    public async Task EliminarTicket_NoCreador_DevuelveForbidden()
    {
        var (_, ticket) = SembrarProyectoConTicket(Guid.NewGuid());
        _currentUser.UsuarioId = Guid.NewGuid();

        var resultado = await CrearService().EliminarTicketAsync(ticket.Id);

        Assert.True(resultado.IsFailure);
        Assert.Equal("Forbidden", resultado.Error.Codigo);
        Assert.Empty(_ticketRepository.Eliminados);
    }

    [Fact]
    public async Task EliminarTicket_CreadorDelProyecto_Elimina()
    {
        var creadorId = Guid.NewGuid();
        var (_, ticket) = SembrarProyectoConTicket(creadorId);
        _currentUser.UsuarioId = creadorId;

        var resultado = await CrearService().EliminarTicketAsync(ticket.Id);

        Assert.True(resultado.IsSuccess);
        Assert.Contains(ticket.Id, _ticketRepository.Eliminados);
        Assert.Equal(1, _unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task EliminarTicket_Inexistente_DevuelveNotFound()
    {
        _currentUser.UsuarioId = Guid.NewGuid();

        var resultado = await CrearService().EliminarTicketAsync(Guid.NewGuid());

        Assert.True(resultado.IsFailure);
        Assert.Equal("NotFound", resultado.Error.Codigo);
    }

    [Fact]
    public async Task ObtenerTicketsPorProyecto_NoMiembro_DevuelveListaVacia()
    {
        var (_, ticket) = SembrarProyectoConTicket(Guid.NewGuid());
        _currentUser.UsuarioId = Guid.NewGuid();
        _currentUser.EsAdministrador = false;

        var resultado = await CrearService().ObtenerTicketsPorProyectoAsync(ticket.ProyectoId!.Value);

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerTicketsPorProyecto_Miembro_DevuelveTickets()
    {
        var creadorId = Guid.NewGuid();
        var (_, ticket) = SembrarProyectoConTicket(creadorId);
        _currentUser.UsuarioId = creadorId;

        var resultado = await CrearService().ObtenerTicketsPorProyectoAsync(ticket.ProyectoId!.Value);

        Assert.Single(resultado);
        Assert.Equal(ticket.Id, resultado[0].Id);
    }
}
