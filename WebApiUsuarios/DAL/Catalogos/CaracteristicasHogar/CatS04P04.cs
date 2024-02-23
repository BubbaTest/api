using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasHogar
{
    public class CatS04P04 //COMO ELIMINAN LA MAYOR PARTE DE LA BASURA
    {
        [Key]
        public decimal CodS04P04 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS04P04 { get; set; }
    }
}
