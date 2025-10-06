using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPP
{
    public class CatCalendario
    {
        [Key]
        public int IdCalendario { get; set; }
        public DateTime Fecha { get; set; }
        public string DiaLaboral { get; set; }
    }
}
