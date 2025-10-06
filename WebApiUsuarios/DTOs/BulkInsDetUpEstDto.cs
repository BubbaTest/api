using Alexa.DAL.IPP;

namespace Alexa.DTOs
{
    public class BulkInsDetUpEstDto
    {
        public IEnumerable<Detalle> Detalle { get; set; } = new List<Detalle>();
        public IEnumerable<CatEstablecimiento> CatEstablecimiento { get; set; } = new List<CatEstablecimiento>();
    }
}
