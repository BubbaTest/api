using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class SEC_EMPLEADO
    {
        [Key]
        public int ID_EMPLEADO { get; set; }
        public bool Activo { get; set; }
        public string USERLOGIN { get; set; }
        public string PASSWD { get; set; }
    }
}
