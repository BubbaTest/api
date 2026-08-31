using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Capacitacion
{
    public class Test
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
