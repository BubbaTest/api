using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CensoEconomico
{
    public class CatSE03P07 //ESTE ESTABLECIMIENTO EMPRESA U HOGAR LLEVA REGISTROS
    {
        [Key]
        public decimal CodSE03P07 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomSE03P07 { get; set; }
    }
}
