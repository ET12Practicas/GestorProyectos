using Microsoft.AspNetCore.Identity;

namespace Aplicacion.Dominio;

public class Usuario : IdentityUser<Guid>
{
    public string Nombre { get; set; } = string.Empty;

    public List<Proyecto> Proyectos { get; set; } = [];

    public List<Proyecto> ProyectosCreados { get; set; } = [];

    public Usuario()
    {
        Id = Guid.NewGuid();
    }

    public Usuario(string unNombre) : this()
    {
        Nombre = unNombre;
        UserName = unNombre;
    }
}
