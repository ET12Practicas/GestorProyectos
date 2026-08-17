using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aplicacion.Dominio;

[Table("Comentario")]
public class Comentario
{
    [Key]
    public Guid IdComentario { get; set; } = Guid.NewGuid();

    [ForeignKey(nameof(UsuarioComentario))]
    public Guid? UsuarioId { get; set; }

    public Usuario? UsuarioComentario { get; set; }

    [InverseProperty(nameof(Aplicacion.Dominio.Ticket.ComentarioTicket))]
    public Ticket? Ticket { get; set; }

    public DateTime FechaComentario { get; set; } = DateTime.UtcNow;

    public string Contenido { get; set; } = string.Empty;

    public Comentario() { }

    public Comentario(string unContenido)
    {
        Contenido = unContenido;
    }
}
