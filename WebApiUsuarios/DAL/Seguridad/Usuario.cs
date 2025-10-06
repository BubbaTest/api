using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
//using Alexa.Validaciones;

namespace Alexa.DAL.Seguridad
{
    [Table("Usuario", Schema = "sde")]
    public class Usuario
    {
        [Key]
        public string UsuarioId { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 50, ErrorMessage = "el campo {0} su longiud es de {1}")]
        //[PrimeraLetraMayuscula]
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Password { get; set; }
        public string Correo { get; set; }
        public Nullable<bool> Activo { get; set; }
        public string? RefreshToken { get; set; }


        //public List<relUsuarioRol> UsuarioRol { get; set; }

    }
}
