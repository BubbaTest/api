using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class Muestra
    {
        [Key]
        public int IdMuestra { get; set; }
        public int ObjIdEstablecimientoCanasta { get; set; }
        public int ObjIdCatVariedad { get; set; }
        public int ObjIdDia { get; set; }
        public string Detalle { get; set; }
        public bool Activo { get; set; }
        public int NVeces { get; set; }
    }
}
