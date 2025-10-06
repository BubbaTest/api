using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

namespace Alexa.DAL.IPP
{
    public class CatValorCatalogo
    {
        [Key]
        public int IdCatValorCatalogo { get; set; }
        public int ObjIdCatCatalogo { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
    }
}
