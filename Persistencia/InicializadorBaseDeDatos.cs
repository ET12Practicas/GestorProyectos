using Microsoft.EntityFrameworkCore;
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

        try
        {
            // Verificamos si la tabla Usuario contiene las columnas de Identity
            await contexto.Database.ExecuteSqlRawAsync("SELECT AccessFailedCount FROM Usuario LIMIT 1;", cancellationToken);
        }
        catch
        {
            // Si la columna no existe o la tabla no existe, significa que la base de datos
            // fue creada previamente con el modelo viejo (sin Identity).
            // Recreamos la base de datos para que aplique el esquema completo de Identity y semillas.
            await contexto.Database.EnsureDeletedAsync(cancellationToken);
            await contexto.Database.EnsureCreatedAsync(cancellationToken);
        }
    }
}
