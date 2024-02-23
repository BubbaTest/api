using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.UsoEdificacion
{
    public class CatS01BP01 //UsoEdificacion
    {
        [Key]
        public decimal CodS01BP01 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS01BP01 { get; set; }
    }
}
