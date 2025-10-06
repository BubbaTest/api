using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPC
{
    public class Muestras
    {
        public string InformanteId { get; set; } = null!;
        public string VariedadId { get; set; } = null!;
        public int? Nveces { get; set; }
    }
}
