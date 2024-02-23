using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P07 //ESTA VIVIENDA TIENE ILUMINARIA DE TIPO
    {
        [Key]
        public decimal CodS03P07 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P07 { get; set; }
    }
}
