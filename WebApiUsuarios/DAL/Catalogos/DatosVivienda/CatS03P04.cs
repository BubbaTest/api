using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P04 //DE QUE MATERIAL ES LA MAYOR PARTE DEL TECHO
    {
        [Key]
        public decimal CodS03P04 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P04 { get; set; }
    }
}
