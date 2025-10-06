using System.ComponentModel.DataAnnotations;

namespace Alexa.DTOs
{
    public class VariedadesDTO
    {
        public string Id { get; set; }
        public string Descripcion { get; set; }
        public string InformanteId { get; set; }
        public int? Semana { get; set; }
        public string Dia { get; set; }
    }
}
