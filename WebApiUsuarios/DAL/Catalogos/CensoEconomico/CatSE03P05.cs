using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CensoEconomico
{
    public class CatSE03P05 //INDIQUE DONDE SE DESARROLLA LA ACTIVIDAD ECONOMICA
    {
        [Key]
        public decimal CodSE03P05 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomSE03P05 { get; set; }
    }
}
