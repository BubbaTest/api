using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P01 //TIPO DE VIVIENDA PARTICULAR
    {
        [Key]
        public decimal CodS03P01 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P01 { get; set; }
    }
}
