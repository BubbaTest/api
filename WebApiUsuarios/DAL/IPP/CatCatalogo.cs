using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace Alexa.DAL.IPP
{
    public class CatCatalogo
    {
        [Key]
        public int IdCatCatalogo { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }
}
