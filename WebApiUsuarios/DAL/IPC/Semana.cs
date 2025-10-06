using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPC
{
    public class Semana
    {
        [Key]
        public int id { get; set; }
        public string descripcion { get; set; }
    }
}
