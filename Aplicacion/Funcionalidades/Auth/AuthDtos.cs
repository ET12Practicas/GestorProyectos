namespace Aplicacion.Funcionalidades.Auth;

public sealed class RegistroUsuarioDto
{
    public required string Nombre { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public sealed class LoginUsuarioDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public bool Recordarme { get; set; } = false;
}

public sealed class UsuarioAutenticadoDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime? ExpiraEn { get; set; }
}

