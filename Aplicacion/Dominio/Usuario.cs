namespace Aplicacion.Dominio;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public List<Proyecto> Proyectos { get; set; } = [];

    public Usuario() { }

    public Usuario(string unNombre)
    {
        Nombre = unNombre;
    }
}
