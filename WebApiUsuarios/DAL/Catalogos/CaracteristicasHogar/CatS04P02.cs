using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasHogar
{
    public class CatS04P02 //EN ESTE HOGAR EL COMBUSTIBLE USADO PRINCIPALMENTE PARA COCINAR ES
    {
        [Key]
        public decimal CodS04P02 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS04P02 { get; set; }
    }
}
