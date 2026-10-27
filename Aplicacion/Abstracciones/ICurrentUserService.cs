namespace Aplicacion.Abstracciones;

public interface ICurrentUserService
{
    Guid? UsuarioId { get; }
    string? Nombre { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
}
