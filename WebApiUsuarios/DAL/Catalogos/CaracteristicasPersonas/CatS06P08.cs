using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P08 //CUAL ES LA RAZON POR LA QUE ES DEPENDIENTE DEL HOGAR
    {
        [Key]
        public decimal CodS06P08 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P08 { get; set; }
    }
}
