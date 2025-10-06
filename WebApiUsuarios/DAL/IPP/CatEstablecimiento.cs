using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class CatEstablecimiento
    {
        [Key]
        public int IdCatEstablecimiento { get; set; }
        public int ObjCodMuni { get; set; }
        public string Razon_soc { get; set; }
        public string Nombre { get; set; }
        public string Encargado { get; set; }
        public string Cargo { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public Nullable<int> DiaHabil { get; set; }
        public double? CoordenadaX { get; set; }
        public double? CoordenadaY { get; set; }
        public bool Activo { get; set; }

        [JsonIgnore]
        public ICollection<EstablecimientoCanasta> EstablecimientosCanasta { get; set; }
    }
}
