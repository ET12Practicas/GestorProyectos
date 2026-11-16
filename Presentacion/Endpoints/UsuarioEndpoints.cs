using Aplicacion.Funcionalidades.Usuarios;
using Carter;

namespace Presentacion.Endpoints;

public sealed class UsuarioEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/usuarios")
            .WithTags("Usuarios")
            .RequireAuthorization("SoloAdministradores");

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

        group.MapDelete("/{usuarioId:guid}", async (IUsuarioService service, Guid usuarioId, CancellationToken ct) =>
        {
            var result = await service.EliminarUsuarioAsync(usuarioId, ct);
            return result.ToHttpResult();
        });

        app.MapGet("/api/usuarios/buscar", async (
            string email,
            IUsuarioService usuarioService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return Results.BadRequest(new { mensaje = "El email de búsqueda es requerido." });
            }

            var usuario = await usuarioService.ObtenerUsuarioPorEmailAsync(email, ct);
            return usuario is not null ? Results.Ok(usuario) : Results.NotFound();
        }).RequireAuthorization().WithTags("Usuarios");
    }
}
