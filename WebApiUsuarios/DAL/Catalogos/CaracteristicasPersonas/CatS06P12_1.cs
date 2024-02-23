using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P12_1 //DIFICULTADES QUE PUEDE TNER UN PERSONA PARA REALIZAR CIERTAS ACTIVIDADES
    {
        [Key]
        public decimal CodS06P12_1 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P12_1 { get; set; }
    }
}
