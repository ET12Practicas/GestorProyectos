using Aplicacion.Funcionalidades.Usuarios;
using Carter;

namespace Presentacion.Endpoints;

public sealed class UsuarioEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/usuarios").WithTags("Usuarios");

        group.MapGet("/", async (IUsuarioService service, CancellationToken ct) =>
        {
            var usuarios = await service.ObtenerUsuariosAsync(ct);
            return Results.Ok(usuarios);
        });

        group.MapGet("/{usuarioId:guid}", async (IUsuarioService service, Guid usuarioId, CancellationToken ct) =>
        {
            var usuario = await service.ObtenerUsuarioPorIdAsync(usuarioId, ct);
            return usuario is not null ? Results.Ok(usuario) : Results.NotFound();
        });

        group.MapPost("/", async (IUsuarioService service, UsuarioCommandDto dto, CancellationToken ct) =>
        {
            var result = await service.CrearUsuarioAsync(dto, ct);
            return result.ToHttpResult($"/api/usuarios/{result.Value}");
        });

        group.MapDelete("/{usuarioId:guid}", async (IUsuarioService service, Guid usuarioId, CancellationToken ct) =>
        {
            var result = await service.EliminarUsuarioAsync(usuarioId, ct);
            return result.ToHttpResult();
        });
    }
}
