using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P03 //DE QUE MATERIAL ES LA MAYOR PARTE DE LAS PAREDES
    {
        [Key]
        public decimal CodS03P03 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P03 { get; set; }

    }
}
