using Aplicacion.Funcionalidades.Usuarios;
using Carter;

namespace Presentacion.Endpoints;

public sealed class UsuarioEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var usuarios = app.MapGroup("/api/usuario").WithTags("Usuarios");

        usuarios.MapGet("/", (IUsuarioService service) =>
            Results.Ok(service.ObtenerUsuarios()));

        usuarios.MapPost("/", (IUsuarioService service, UsuarioCommandDto dto) =>
            service.CrearUsuario(dto)
                ? Results.Created("/api/usuario", null)
                : Results.BadRequest("El nombre del usuario es obligatorio."));

        usuarios.MapDelete("/{usuarioId:guid}", (IUsuarioService service, Guid usuarioId) =>
            service.EliminarUsuario(usuarioId)
                ? Results.NoContent()
                : Results.NotFound());
    }
}
