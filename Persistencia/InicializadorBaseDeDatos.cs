using Microsoft.Extensions.DependencyInjection;

namespace Persistencia;

public static class InicializadorBaseDeDatos
{
    public static async Task InicializarBaseDeDatosAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var contexto = scope.ServiceProvider.GetRequiredService<ProyectoDbContext>();

        await contexto.Database.EnsureCreatedAsync(cancellationToken);
    }
}
