using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Aplicacion.Enums;

namespace Aplicacion.Dominio;

[Table("Ticket")]
public class Ticket
{
    [Key]
    [Required]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(45)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public DateTime FechaTicket { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UsuarioTicket))]
    public Guid? UsuarioId { get; set; }

    public Usuario? UsuarioTicket { get; set; }

    public EEstadoTicket Estado { get; set; } = EEstadoTicket.Abierto;

    [ForeignKey(nameof(ComentarioTicket))]
    public Guid? ComentarioId { get; set; }

    [InverseProperty(nameof(Comentario.Ticket))]
    public Comentario? ComentarioTicket { get; set; }

    [ForeignKey(nameof(ProyectoTicket))]
    public Guid? ProyectoId { get; set; }

    [InverseProperty(nameof(Proyecto.Tickets))]
    public Proyecto? ProyectoTicket { get; set; }

    public Ticket() { }

    public Ticket(string nombre)
    {
        Nombre = nombre;
    }

    public void AgregarUsuario(Usuario usuario) => UsuarioTicket = usuario;

    public void AgregarComentario(Comentario comentario) => ComentarioTicket = comentario;
}
