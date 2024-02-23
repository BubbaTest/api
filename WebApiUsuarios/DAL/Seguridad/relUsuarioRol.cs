using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Seguridad
{
    [Table("relUsuarioRol", Schema = "sde")]
    public class relUsuarioRol
    {

        public string UsuarioId { get; set; }

        public string RolId { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public Guid relGUID { get; set; }

        //public Usuario Usuario { get; set; }
    }
}
