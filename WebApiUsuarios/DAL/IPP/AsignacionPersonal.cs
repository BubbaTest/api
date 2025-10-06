using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class AsignacionPersonal
    {
        [Key]
        public int IdAsignacionPersonal { get; set; }
        public int ObjIdEstablecimientoCanasta { get; set; }
        public int ObjIdCatPersonal { get; set; }
        public bool Activo { get; set; }
    }
}
