using Aplicacion.Enums;

namespace Aplicacion.Dominio;

public class Ticket
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaTicket { get; set; } = DateTime.UtcNow;

    public EEstadoTicket Estado { get; set; } = EEstadoTicket.Abierto;

    public Guid? UsuarioId { get; set; }

    public Usuario? UsuarioTicket { get; set; }

    public Guid? ProyectoId { get; set; }

    public Proyecto? ProyectoTicket { get; set; }

    public List<Comentario> Comentarios { get; set; } = [];

    public Ticket() { }

    public Ticket(string nombre, string descripcion = "")
    {
        Nombre = nombre;
        Descripcion = descripcion;
    }

    public void AgregarUsuario(Usuario usuario) => UsuarioTicket = usuario;

    public void AgregarComentario(Comentario comentario) => Comentarios.Add(comentario);
}
