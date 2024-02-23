using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P16 //CUAL ES LA RAZON PRINCIPAL POR LA QUE NO ASISTE ACTUALMENTE A LA ESCUELA
    {
        [Key]
        public decimal CodS06P16 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P16 { get; set; }
    }
}
