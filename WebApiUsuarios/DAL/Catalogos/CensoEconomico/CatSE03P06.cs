using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CensoEconomico
{
    public class CatSE03P06 //CUAL ES LA CONDICION JURIDICA
    {
        [Key]
        public decimal CodSE03P06 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomSE03P06 { get; set; }
    }
}
