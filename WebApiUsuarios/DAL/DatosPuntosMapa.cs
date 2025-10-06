using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL
{
    [Table("DatosPuntosMapa", Schema = "dbo")]
    public class DatosPuntosMapa
    {
        [Key]
        public int Id { get; set; }
        public string Etiqueta { get; set; }
        public decimal longitude { get; set; }
        public decimal latitude { get; set; }
        public string tipo { get; set; }

    }
}
