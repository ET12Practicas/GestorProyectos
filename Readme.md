# Gestor de proyectos

Solución ASP.NET Core organizada en tres capas:

- `Presentacion`: host web Blazor, endpoints HTTP y configuración.
- `Aplicacion`: entidades de dominio, DTO, casos de uso y contratos de persistencia.
- `Persistencia`: Entity Framework Core, MySQL y repositorios.

Las dependencias quedan orientadas hacia la aplicación:

```text
Presentacion ──> Aplicacion <── Persistencia
      │                            ▲
      └────────────────────────────┘
          composición de dependencias
```

## Ejecución

1. Configurar `ConnectionStrings:proyecto_db` en
   `Presentacion/appsettings.json` o mediante secretos de usuario.
2. Ejecutar `dotnet run --project Presentacion`.
3. En desarrollo, Swagger queda disponible en `/swagger`.
