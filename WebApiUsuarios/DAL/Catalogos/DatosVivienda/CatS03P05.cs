using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P05 //DE QUE MATERIAL ES LA MAYOR PARTE DEL PISO
    {
        [Key]
        public decimal CodS03P05 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P05 { get; set; }
    }
}
