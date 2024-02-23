using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P17_1 //CUAL ES EL GRADO O AÑO ESCOLAR MAS ALTO QUE APROBO
    {
        [Key]
        public decimal CodS06P17_1 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P17_1 { get; set; }
    }
}
