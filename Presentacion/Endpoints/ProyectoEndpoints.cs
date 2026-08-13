using Aplicacion.Enums;
using Aplicacion.Funcionalidades.Comentarios;
using Aplicacion.Funcionalidades.Proyectos;
using Aplicacion.Funcionalidades.Tickets;
using Carter;

namespace Presentacion.Endpoints;

public sealed class ProyectoEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/proyecto", (IProyectoService service) =>
            Results.Ok(service.ObtenerProyectos()))
            .WithTags("Proyectos");

        app.MapPost("/api/proyecto", (IProyectoService service, ProyectoCommandDto dto) =>
            service.CrearProyecto(dto)
                ? Results.Created("/api/proyecto", null)
                : Results.BadRequest("El nombre del proyecto es obligatorio."))
            .WithTags("Proyectos");

        app.MapPost(
                "/api/proyecto/{proyectoId:guid}/ticket",
                (IProyectoService service, Guid proyectoId, TicketCommandDto dto) =>
                    service.CrearTicket(proyectoId, dto)
                        ? Results.Created($"/api/proyecto/{proyectoId}/ticket", null)
                        : Results.BadRequest("El proyecto no existe o el nombre es inválido."))
            .WithTags("Proyectos");

        app.MapDelete("/api/ticket/{ticketId:guid}", (IProyectoService service, Guid ticketId) =>
            service.EliminarTicket(ticketId) ? Results.NoContent() : Results.NotFound())
            .WithTags("Tickets");

        app.MapPut(
                "/api/ticket/{ticketId:guid}/usuario/{usuarioId:guid}",
                (IProyectoService service, Guid ticketId, Guid usuarioId) =>
                    service.AsignarUsuarioATicket(ticketId, usuarioId)
                        ? Results.NoContent()
                        : Results.BadRequest("El usuario y el ticket deben pertenecer al mismo proyecto."))
            .WithTags("Tickets");

        app.MapPut(
                "/api/ticket/{ticketId:guid}/estado",
                (IProyectoService service, Guid ticketId, EEstadoTicket estado) =>
                    service.CambiarEstadoTicket(ticketId, new EstadoTicketDto { Estado = estado })
                        ? Results.NoContent()
                        : Results.BadRequest("El ticket no existe o el estado es inválido."))
            .WithTags("Tickets");

        app.MapDelete(
                "/api/usuario/{usuarioId:guid}/ticket/{ticketId:guid}",
                (IProyectoService service, Guid usuarioId, Guid ticketId) =>
                    service.QuitarUsuarioDeProyectos(usuarioId, ticketId)
                        ? Results.NoContent()
                        : Results.NotFound())
            .WithTags("Usuarios");

        app.MapPost(
                "/api/proyecto/{proyectoId:guid}/usuario/{usuarioId:guid}",
                (IProyectoService service, Guid proyectoId, Guid usuarioId) =>
                    service.AsignarUsuarioAProyecto(usuarioId, proyectoId)
                        ? Results.NoContent()
                        : Results.NotFound())
            .WithTags("Proyectos");

        app.MapPost(
                "/api/usuario/{usuarioId:guid}/ticket/{ticketId:guid}/comentario",
                (IProyectoService service, Guid usuarioId, Guid ticketId, ComentarioCommandDto dto) =>
                    service.CrearComentario(ticketId, usuarioId, dto)
                        ? Results.Created($"/api/usuario/{usuarioId}/ticket/{ticketId}/comentario", null)
                        : Results.BadRequest("El usuario y el ticket deben pertenecer al mismo proyecto."))
            .WithTags("Comentarios");
    }
}
