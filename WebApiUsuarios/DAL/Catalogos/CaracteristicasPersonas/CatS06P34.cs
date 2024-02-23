using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P34 //ESTA VIVO O MUERTO SU ULTIMO HIJO NACIDO
    {
        [Key]
        public decimal CodS06P34 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P34 { get; set; }
    }
}
