using Aplicacion.Funcionalidades.Usuarios;

namespace Aplicacion.Funcionalidades.Comentarios;

public sealed class ComentarioCommandDto
{
    public required string Contenido { get; set; }
}

public sealed class ComentarioQueryDto
{
    public Guid Id { get; set; }
    public UsuarioQueryDto? Usuario { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}
