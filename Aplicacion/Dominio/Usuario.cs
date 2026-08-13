using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aplicacion.Dominio;

[Table("Usuario")]
public class Usuario
{
    [Key]
    [Required]
    public Guid Id { get; set;} = Guid.NewGuid();
    
    [Required]
    [StringLength(45)]
    public string Nombre { get; set;} = string.Empty;

    [ForeignKey(nameof(ProyectoUsuario))]
    public Guid? ProyectoId { get; set; }

    [InverseProperty(nameof(Proyecto.Usuarios))]
    public Proyecto? ProyectoUsuario { get; set; }

    public Usuario() { }

    public Usuario(string unNombre)
    {
        Nombre = unNombre;
    }
}
