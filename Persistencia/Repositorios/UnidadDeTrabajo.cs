using Aplicacion.Abstracciones;

namespace Persistencia;

internal sealed class UnidadDeTrabajo(ProyectoDbContext context) : IUnidadDeTrabajo
{
    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);
}
