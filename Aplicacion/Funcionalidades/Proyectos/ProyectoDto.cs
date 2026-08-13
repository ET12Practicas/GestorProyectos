using Aplicacion.Funcionalidades.Tickets;
using Aplicacion.Funcionalidades.Usuarios;

namespace Aplicacion.Funcionalidades.Proyectos;

public sealed class ProyectoCommandDto
{
    public required string Nombre { get; set; }
}

public sealed class ProyectoQueryDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public IReadOnlyList<TicketQueryDto> Tickets { get; set; } = [];
    public IReadOnlyList<UsuarioQueryDto> Usuarios { get; set; } = [];
}
