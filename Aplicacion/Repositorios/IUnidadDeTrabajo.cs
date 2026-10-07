namespace Aplicacion.Abstracciones;

public interface IUnidadDeTrabajo
{
    Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
