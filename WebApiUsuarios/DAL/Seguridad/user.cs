using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Seguridad
{
    public class user
    {
        [Key]
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
