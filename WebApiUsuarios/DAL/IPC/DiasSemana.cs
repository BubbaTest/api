using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPC
{
    public class DiasSemana
    {
        [Key]
        public string IDdia { get; set; }
        public string Dia { get; set; }
        public string Orden { get; set; }
    }
}
