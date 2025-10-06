using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class CatCanasta
    {
        [Key]
        public int IdCatCanasta { get; set; }
        public int ObjIdCatEncuesta { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }
}
