using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class CatVariedad
    {
        [Key]
        public int IdCatVariedad { get; set; }
        public int ObjIdCatCanasta { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
