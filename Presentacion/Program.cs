using System.Text.Json.Serialization;
using Aplicacion;
using Carter;
using Persistencia;
using Presentacion.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCarter();
builder.Services.AddAplicacion();

var connectionString = builder.Configuration.GetConnectionString("proyecto_db")
    ?? throw new InvalidOperationException(
        "No se configuró la cadena de conexión 'proyecto_db'.");

builder.Services.AddPersistencia(connectionString);

var app = builder.Build();

await app.Services.InicializarBaseDeDatosAsync();

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
app.UseAntiforgery();
app.MapStaticAssets();
app.MapCarter();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
