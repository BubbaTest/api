using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos
{
    public class CatSiNo
    {
        [Key]
        public decimal CodSiNo { get; set; }
        public string NomSiNo { get; set; }
    }
}
