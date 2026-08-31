using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class MuestraM
    {
        public int ObjIdEstablecimientoCanasta { get; set; }
        public int ObjIdCatVariedad { get; set; }
        public string Detalle { get; set; }
        public bool Activo { get; set; }
        public string muestraid { get; set; }
    }
}
