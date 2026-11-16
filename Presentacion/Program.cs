using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Aplicacion;
using Aplicacion.Abstracciones;
using Dominio.Entidades;
using Aplicacion.Funcionalidades.Auth;
using Carter;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Persistencia;
using Presentacion.Components;
using Presentacion.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gestor de Proyectos API",
        Version = "v1",
        Description = "API de gestión de proyectos y tickets con soporte híbrido de autenticación (JWT y Cookies de Identity)."
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresá tu token JWT en el formato: Bearer {tu_token} o directamente el token."
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

builder.Services.AddCarter();
builder.Services.AddAplicacion();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var connectionString = builder.Configuration.GetConnectionString("proyecto_db");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se configuró la cadena de conexión 'proyecto_db'. Definila mediante user-secrets o la variable de entorno ConnectionStrings__proyecto_db.");
}

builder.Services.AddPersistencia(connectionString);

// Autenticación híbrida: Cookies de Identity + JWT Bearer
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSection["ClaveSecreta"];
if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException(
        "No se configuró 'Jwt:ClaveSecreta'. Definila mediante user-secrets o la variable de entorno Jwt__ClaveSecreta.");
}

var keyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

builder.Services.AddAuthentication()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Emisor"] ?? "GestorProyectosApi",
            ValidAudience = jwtSection["Audiencia"] ?? "GestorProyectosClient",
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddIdentityCore<Usuario>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.Lockout.AllowedForNewUsers = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<ProyectoDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    // Ambas políticas aceptan los dos esquemas (JWT Bearer y Cookie de Identity)
    var esquemaDoble = new[] { JwtBearerDefaults.AuthenticationScheme, IdentityConstants.ApplicationScheme };
    options.AddPolicy("SoloAdministradores", p =>
        p.AddAuthenticationSchemes(esquemaDoble).RequireRole("Administrador"));
    options.DefaultPolicy = new AuthorizationPolicyBuilder(esquemaDoble)
        .RequireAuthenticatedUser()
        .Build();
});

// Las rutas de API devuelven 401/403 en lugar de redirigir al login del navegador
builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Se escribe el cuerpo en el rechazo para que UseStatusCodePagesWithReExecute no re-ejecute el pipeline.
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            """{"error":"Demasiados intentos. Esperá un momento y volvé a probar."}""",
            cancellationToken);
    };
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

await app.Services.InicializarBaseDeDatosAsync(app.Environment.IsDevelopment());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(x => x.EnableTryItOutByDefault());
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapCarter();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
