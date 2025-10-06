using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class CatUnidadMedida
    {
        [Key]
        public int IdCatUnidadMedida { get; set; }
        public string Nombre { get; set; }
    }
}
