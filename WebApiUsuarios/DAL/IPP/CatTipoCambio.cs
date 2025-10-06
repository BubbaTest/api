using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class CatTipoCambio
    {
        [Key]
        public int IdCambio { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Cambio { get; set; }
    }
}
