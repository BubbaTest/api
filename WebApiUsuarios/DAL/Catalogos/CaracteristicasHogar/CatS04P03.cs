using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasHogar
{
    public class CatS04P03 //EL TIPO DE SERVICIO HIGIENICO QUE TIENE ES
    {
        [Key]
        public decimal CodS04P03 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS04P03 { get; set; }
    }
}
