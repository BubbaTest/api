using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
namespace Alexa.DTOs
{
    public class RespuestaAutenticacion
    {
        public string Token { get; set; }
        public DateTime Expiracion { get; set; }
        //public string RolDescripcion { get; set; }
        //public string RolId { get; set; }
        //public string Correo { get; set; }
        //public string Nombres { get; set; }
        //public string Apellidos { get; set; }
    }
}
