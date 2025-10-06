using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPC
{
    public class LoginUsuarios
    {
        [Key]
        public int IdUsuario { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Usuario { get; set; }
        public string IdEmpleado { get; set; }
        public bool Activo { get; set; }
        public string Pass { get; set; }
    }
}
