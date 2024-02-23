using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P05 //QUE IDIOMAS O LENGUAS HABLA
    {
        [Key]
        public decimal CodS06P05 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P05 { get; set; }
    }
}
