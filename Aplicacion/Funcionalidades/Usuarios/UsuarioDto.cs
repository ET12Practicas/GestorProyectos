namespace Aplicacion.Funcionalidades.Usuarios;

public sealed class UsuarioCommandDto
{
    public required string Nombre { get; set; }
}

public sealed class UsuarioQueryDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
