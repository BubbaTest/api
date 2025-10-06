using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPC
{
    public class RegionDistrito
    {
        [Key]
        public int RegionDistritoId { get; set; }
        public string NomRegionDistrito { get; set; }
    }
}
