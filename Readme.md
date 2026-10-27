# Gestor de proyectos

Solución ASP.NET Core organizada en cuatro capas:

- `Presentacion`: host web Blazor, endpoints HTTP y configuración.
- `Aplicacion`: DTO, casos de uso, validaciones y contratos de persistencia.
- `Dominio`: entidades (`Entidades`) y enumeraciones (`Enums`).
- `Persistencia`: Entity Framework Core, MySQL y repositorios.

## Estructura del proyecto

```text
GestorProyectos/
├── GestorProyecto.slnx
├── Dominio/
│   ├── Dominio.csproj
│   ├── Entidades/
│   │   ├── Comentario.cs
│   │   ├── Proyecto.cs
│   │   ├── Ticket.cs
│   │   └── Usuario.cs
│   └── Enums/
│       └── EEstadoTicket.cs
├── Aplicacion/
│   ├── Aplicacion.csproj
│   ├── Abstracciones/             # Contrato de acceso al usuario actual
│   ├── Comun/                     # Resultados y errores de los casos de uso
│   ├── Funcionalidades/
│   │   ├── Auth/                  # DTO, validadores y contratos de autenticación
│   │   ├── Comentarios/           # DTO, validadores y servicios de comentarios
│   │   ├── Proyectos/             # DTO, validadores y servicios de proyectos
│   │   ├── Tickets/               # DTO, validadores y servicios de tickets
│   │   └── Usuarios/              # DTO, validadores y servicios de usuarios
│   ├── Repositorios/              # Interfaces de repositorios y unidad de trabajo
│   └── DependencyInjection.cs
├── Persistencia/
│   ├── Persistencia.csproj
│   ├── Configuraciones/           # Mapeos de entidades con EF Core
│   ├── Repositorios/              # Repositorios y unidad de trabajo con EF Core
│   ├── ProyectoDbContext.cs
│   ├── InicializadorBaseDeDatos.cs
│   └── DependencyInjection.cs
├── Presentacion/
│   ├── Presentacion.csproj
│   ├── Components/
│   │   ├── Layout/                # Diseño y navegación de la interfaz
│   │   ├── Pages/                 # Páginas Blazor
│   │   ├── App.razor
│   │   ├── Routes.razor
│   │   └── _Imports.razor
│   ├── Endpoints/                 # Rutas HTTP y conversión de resultados
│   ├── Servicios/                 # Autenticación, JWT y usuario actual
│   ├── Properties/                # Perfiles de ejecución
│   ├── wwwroot/                   # Estilos, imágenes y bibliotecas estáticas
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Presentacion.http
│   └── Program.cs
└── Readme.md
```

El árbol muestra las carpetas y archivos principales; omite los directorios
generados `bin` y `obj` y los archivos de configuración del editor.

## Dependencias entre capas

Las dependencias entre proyectos son:

```text
Aplicacion   -> Dominio
Persistencia -> Aplicacion, Dominio
Presentacion -> Aplicacion, Dominio, Persistencia
```

`Presentacion` referencia `Persistencia` para componer las dependencias en el
arranque. `Dominio` no referencia otros proyectos de la solución; `Usuario`
conserva su integración con ASP.NET Core Identity.

## Ejecución

1. Configurar `ConnectionStrings:proyecto_db` en
   `Presentacion/appsettings.json` o mediante secretos de usuario.
2. Ejecutar `dotnet run --project Presentacion`.
3. En desarrollo, Swagger queda disponible en `/swagger`.
