using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class CatUMedVar
    {
        [Key]
        public int IdCatUMedVar { get; set; }
        public int ObjIdCatVariedad { get; set; }
        public int ObjURecolId { get; set; }
    }
}
