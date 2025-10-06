using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Alexa.DAL.IPP
{
    public class SEC_MUNI
    {
        [Key]
        public Int64 ID_Muni { get; set; }
        public string NOM_MUNI { get; set; }
    }
}
