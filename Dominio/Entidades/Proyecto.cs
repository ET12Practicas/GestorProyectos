namespace Dominio.Entidades;

public class Proyecto
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public Guid? CreadorId { get; set; }

    public Usuario? Creador { get; set; }

    public List<Usuario> Usuarios { get; set; } = [];

    public List<Ticket> Tickets { get; set; } = [];

    public Proyecto() { }

    public Proyecto(string nombre)
    {
        Nombre = nombre;
    }

    public Proyecto(string nombre, Guid? creadorId) : this(nombre)
    {
        CreadorId = creadorId;
    }
}
