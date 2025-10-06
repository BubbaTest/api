using Alexa.DAL.IPC;

namespace Alexa.DTOs
{
    public class BulkInsSPUpMDto
    {
        public IEnumerable<SeriesPrecios> SeriesPrecios { get; set; } = new List<SeriesPrecios>();
        public IEnumerable<Muestras> Muestras { get; set; } = new List<Muestras>();
        public IEnumerable<CampoInformantes> Informantes { get; set; } = new List<CampoInformantes>();
        //public IEnumerable<Informantes> Informantes { get; set; } = new List<Informantes>();
    }
}
