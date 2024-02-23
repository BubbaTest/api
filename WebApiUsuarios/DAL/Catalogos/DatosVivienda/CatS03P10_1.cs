using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P10_1 //CUANTOS AÑOS DE ANTIGUEDAD TIENE ESTA VIVIENDA
    {
        [Key]
        public decimal CodS03P10_1 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P10_1 { get; set; }
    }
}
