using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alexa.DAL.IPP
{
    public class Detalle
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int IdDetalle { get; set; }
        public int ObjIdCatCanasta { get; set; }
        public int ObjCodMuni { get; set; }

        public int ObjIdEstablecimientoCanasta { get; set; }
        public int ObjIdCatVariedad { get; set; }
        public DateTime FechaDefinidaRecoleccion { get; set; }
        public string muestraid { get; set; }
        public double PrecioCalculado { get; set; }
        public double? PrecioRealRecolectado { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaRecoleccion { get; set; }
        public double? TasaCambio { get; set; }
        public Nullable<int> ObjIdTipoMoneda { get; set; }
        public int ObjIdEstadoVar { get; set; }
        public Nullable<int> ObjIdUnidRecolectada { get; set; }
        public Nullable<DateTime> FechaImputado { get; set; }
        public string? Observacion { get; set; }
        public string? UsuarioCreacion { get; set; }
    }
}
