using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class EstablecimientoCanasta
    {
        [Key]
        public int IdEstablecimientoCanasta { get; set; }
        public int ObjIdCatEstablecimiento { get; set; }

        public CatEstablecimiento CatEstablecimiento { get; set; }
        public int ObjIdCatCanasta { get; set; }
    }
}
