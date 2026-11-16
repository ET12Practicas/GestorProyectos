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

`appsettings.json` no contiene secretos: la cadena de conexión y la clave JWT se
leen de user-secrets o variables de entorno, y la aplicación falla al arrancar si
faltan. En cada máquina hay que configurarlos una vez:

```bash
dotnet user-secrets --project Presentacion set "ConnectionStrings:proyecto_db" "server=localhost;database=5to_gestorproyectos;user=TU_USUARIO;password=TU_CLAVE"
dotnet user-secrets --project Presentacion set "Jwt:ClaveSecreta" "TU_CLAVE_ALEATORIA_DE_32_O_MAS_CARACTERES"
dotnet user-secrets --project Presentacion set "Administracion:EmailsAdministradores:0" "admin@tu-dominio.com"
```

1. Con los secretos cargados, ejecutar `dotnet run --project Presentacion`.
2. La base se crea/actualiza automáticamente al iniciar (migraciones).
3. El email configurado en `Administracion:EmailsAdministradores` recibe el rol
   `Administrador` al arrancar, pero solo si el usuario ya existe: registralo
   primero y reiniciá la aplicación.
4. En desarrollo, Swagger queda disponible en `/swagger`.

## Releases e imagen Docker

El workflow `.github/workflows/release-main.yml` se ejecuta con cada push a
`main`, incluidos los que genera un merge hacia esa rama. Los pushes directos
a `main` también publican un release.

- La primera versión es `v1.0.0`; cada publicación incrementa el patch
  (`v1.0.1`, `v1.0.2`, etc.) a partir del mayor tag estable `vX.Y.Z` existente.
- El release apunta al commit integrado e incluye el título y el hash enlazado
  de cada commit desde el tag anterior. El primer release incluye todo el historial.
- El Dockerfile compila `Presentacion` y sus dependencias con .NET 10, y utiliza
  la imagen de ASP.NET Core 10 para ejecutar la aplicación en el puerto 8080.
- La imagen se publica en `ghcr.io/et12practicas/gestorproyectos:vX.Y.Z`
  y `ghcr.io/et12practicas/gestorproyectos:latest`.
- El release se publica después de subir la imagen. Reejecutar el workflow
  reutiliza el tag del mismo commit; reejecutar una versión anterior no reemplaza
  la etiqueta `latest`.

La autenticación utiliza el `GITHUB_TOKEN` automático con permisos
`contents: write` y `packages: write`; no requiere un PAT. La cuenta ET12Practicas debe permitir que este repositorio
publique paquetes. Si el paquete ya existe, otorgar al repositorio acceso de
escritura en la configuración del paquete (Manage Actions access).
Ver [autenticación y acceso a GHCR](https://docs.github.com/en/packages/working-with-a-github-packages-registry/working-with-the-container-registry).

Para ejecutar la imagen, configurar al menos `ConnectionStrings__proyecto_db`
y `Jwt__ClaveSecreta` mediante variables de entorno; MySQL debe ser accesible
desde el contenedor. Los archivos `.env` y `appsettings.Development.json` quedan
fuera del contexto Docker. El Compose de la siguiente sección configura estas
variables automáticamente.

## Docker Compose

El archivo `compose.yaml` compila la aplicación con el Dockerfile y levanta
MySQL 8.4. Para un clon nuevo, copiar `.env.example` a `.env` y configurar
`MYSQL_PASSWORD`, `MYSQL_ROOT_PASSWORD` y `JWT_SECRET` (al menos 32 caracteres).
En PowerShell:

```powershell
Copy-Item .env.example .env
```

El archivo `.env` está excluido de Git y del contexto de construcción de Docker.

Levantar los servicios:

```sh
docker compose up --build -d
```

- Aplicación: <http://localhost:8080>.
- MySQL desde la máquina: `localhost:3307`, con la base y el usuario definidos
  en `.env` (por defecto, `gestorproyectos` y `gestor`).
- La aplicación usa `mysql:3306` dentro de la red de Docker y espera a que
  MySQL responda consultas con su usuario antes de iniciar. El esquema se
  inicializa al arrancar la aplicación.
- Los datos se guardan en el volumen `mysql_data` y se conservan al detener
  los contenedores. Las variables de inicialización de MySQL se aplican al
  crear un volumen nuevo; editarlas no cambia las credenciales de uno existente.

Ver logs y detener los servicios:

```sh
docker compose logs -f
docker compose down
```
