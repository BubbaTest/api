using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class UsuarioIPP
    {
        [Key]
        public string UsuarioId { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Password { get; set; }
        public bool Activo { get; set; }
        public int ID_EMPLEADO { get; set; }
    }
}
