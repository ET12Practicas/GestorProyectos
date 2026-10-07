namespace Aplicacion.Dominio;

public class Proyecto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public List<Usuario> Usuarios { get; set; } = [];

    public List<Ticket> Tickets { get; set; } = [];

    public Proyecto() { }

    public Proyecto(string nombre)
    {
        Nombre = nombre;
    }
}
