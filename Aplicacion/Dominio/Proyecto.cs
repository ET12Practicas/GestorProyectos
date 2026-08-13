using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aplicacion.Dominio;

[Table("Proyecto")]
public class Proyecto
{
    [Key]
    [Required]
    public Guid IdProject { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(45)]
    public string Nombre { get; set; } = string.Empty;

    [InverseProperty(nameof(Usuario.ProyectoUsuario))]
    public List<Usuario> Usuarios { get; set; } = [];

    [InverseProperty(nameof(Ticket.ProyectoTicket))]
    public List<Ticket> Tickets { get; set; } = [];

    public Proyecto() { }

    public Proyecto(string nombre)
    {
        Nombre = nombre;
    }
}
