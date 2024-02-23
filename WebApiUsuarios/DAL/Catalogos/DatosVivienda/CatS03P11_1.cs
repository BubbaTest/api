using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P11_1 //CUAL ES EL VALOR DE ESTA VIVIENDA
    {
        [Key]
        public decimal CodS03P11_1 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P11_1 { get; set; }
    }
}
