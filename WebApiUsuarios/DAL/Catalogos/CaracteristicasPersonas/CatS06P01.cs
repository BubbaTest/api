using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P01 //QUE PARENTESCO TIENE
    {
        [Key]
        public decimal CodS06P01 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P01 { get; set; }
    }
}
