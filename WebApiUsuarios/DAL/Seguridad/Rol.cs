using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Seguridad
{
    [Table("Rol", Schema = "sde")]
    public class Rol
    {
        [Key]
        public string RolId { get; set; }
        public string Descripcion { get; set; }
        public Nullable<bool> Activo { get; set; }

    }
}
