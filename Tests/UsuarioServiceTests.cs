using Aplicacion.Funcionalidades.Usuarios;
using Dominio.Entidades;
using Tests.Fakes;
using Xunit;

namespace Tests;

public class UsuarioServiceTests
{
    private readonly FakeUsuarioRepository _usuarioRepository = new();
    private readonly FakeProyectoRepository _proyectoRepository = new();
    private readonly FakeTicketRepository _ticketRepository = new();
    private readonly FakeUnidadDeTrabajo _unidadDeTrabajo = new();
    private readonly FakeCurrentUserService _currentUser = new();

    private UsuarioService CrearService() => new(
        _usuarioRepository,
        _proyectoRepository,
        _ticketRepository,
        _unidadDeTrabajo,
        _currentUser);

    private static Usuario NuevoUsuario(string nombre = "Usuario")
    {
        var usuario = new Usuario(nombre) { Email = $"{nombre.ToLowerInvariant()}@demo.com" };
        usuario.NormalizedEmail = usuario.Email.ToUpperInvariant();
        return usuario;
    }

    [Fact]
    public async Task ObtenerUsuarios_SinAdmin_DevuelveListaVacia()
    {
        _currentUser.UsuarioId = Guid.NewGuid();
        _currentUser.EsAdministrador = false;
        _usuarioRepository.Usuarios.Add(NuevoUsuario());

        var resultado = await CrearService().ObtenerUsuariosAsync();

        Assert.Empty(resultado);
    }

    [Fact]
    public async Task ObtenerUsuarios_Admin_DevuelveTodos()
    {
        _currentUser.UsuarioId = Guid.NewGuid();
        _currentUser.EsAdministrador = true;
        _usuarioRepository.Usuarios.Add(NuevoUsuario("uno"));
        _usuarioRepository.Usuarios.Add(NuevoUsuario("dos"));

        var resultado = await CrearService().ObtenerUsuariosAsync();

        Assert.Equal(2, resultado.Count);
    }

    [Fact]
    public async Task EliminarUsuario_SinAdmin_DevuelveForbidden()
    {
        var victima = NuevoUsuario();
        _usuarioRepository.Usuarios.Add(victima);
        _currentUser.UsuarioId = Guid.NewGuid();
        _currentUser.EsAdministrador = false;

        var resultado = await CrearService().EliminarUsuarioAsync(victima.Id);

        Assert.True(resultado.IsFailure);
        Assert.Equal("Forbidden", resultado.Error.Codigo);
        Assert.Empty(_usuarioRepository.Eliminados);
    }

    [Fact]
    public async Task EliminarUsuario_AdminSobreSiMismo_DevuelveConflict()
    {
        var admin = NuevoUsuario("admin");
        _usuarioRepository.Usuarios.Add(admin);
        _currentUser.UsuarioId = admin.Id;
        _currentUser.EsAdministrador = true;

        var resultado = await CrearService().EliminarUsuarioAsync(admin.Id);

        Assert.True(resultado.IsFailure);
        Assert.Equal("Conflict", resultado.Error.Codigo);
        Assert.Empty(_usuarioRepository.Eliminados);
    }

    [Fact]
    public async Task EliminarUsuario_AdminSobreOtro_EliminaYDesvincula()
    {
        var admin = NuevoUsuario("admin");
        var victima = NuevoUsuario("victima");
        _usuarioRepository.Usuarios.Add(admin);
        _usuarioRepository.Usuarios.Add(victima);

        var proyecto = new Proyecto("Proyecto", victima.Id);
        proyecto.Usuarios.Add(victima);
        var ticket = new Ticket("Ticket") { ProyectoId = proyecto.Id, ProyectoTicket = proyecto, UsuarioId = victima.Id };
        proyecto.Tickets.Add(ticket);
        _proyectoRepository.Proyectos.Add(proyecto);
        _ticketRepository.Tickets.Add(ticket);

        _currentUser.UsuarioId = admin.Id;
        _currentUser.EsAdministrador = true;

        var resultado = await CrearService().EliminarUsuarioAsync(victima.Id);

        Assert.True(resultado.IsSuccess);
        Assert.Contains(victima.Id, _usuarioRepository.Eliminados);
        Assert.Null(proyecto.CreadorId);
        Assert.DoesNotContain(victima, proyecto.Usuarios);
        Assert.Null(ticket.UsuarioId);
        Assert.Equal(1, _unidadDeTrabajo.Guardados);
    }

    [Fact]
    public async Task EliminarUsuario_Inexistente_DevuelveNotFound()
    {
        _currentUser.UsuarioId = Guid.NewGuid();
        _currentUser.EsAdministrador = true;

        var resultado = await CrearService().EliminarUsuarioAsync(Guid.NewGuid());

        Assert.True(resultado.IsFailure);
        Assert.Equal("NotFound", resultado.Error.Codigo);
    }
}
