using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Seguridad
{
    public class SessionLog
    {
        [Key]
        public string UsuarioId { get; set; }
        public string Sistema { get; set; }
        public string Sesion { get; set; }
        public bool Activo { get; set; }
    }
}
