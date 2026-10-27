namespace Dominio.Entidades;

public class Comentario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Contenido { get; set; } = string.Empty;

    public DateTime FechaComentario { get; set; } = DateTime.UtcNow;

    public Guid? UsuarioId { get; set; }

    public Usuario? UsuarioComentario { get; set; }

    public Guid TicketId { get; set; }

    public Ticket? Ticket { get; set; }

    public Comentario() { }

    public Comentario(string unContenido)
    {
        Contenido = unContenido;
    }
}
