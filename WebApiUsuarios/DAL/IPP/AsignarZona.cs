using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class AsignarZona
    {
        [Key]
        public int IdAsignarZona { get; set; }
        public int ObjIdCatPersonal { get; set; }
        public int ObjIdMuni { get; set; }
        public bool Activo { get; set; }
    }
}
