using Aplicacion.Abstracciones;

namespace Persistencia;

internal sealed class UnidadDeTrabajo(ProyectoDbContext context) : IUnidadDeTrabajo
{
    public void GuardarCambios() => context.SaveChanges();
}
