using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P24 //EL TRABAJO QUE REALIZA ES
    {
        [Key]
        public decimal CodS06P24 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P24 { get; set; }
    }
}
