using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P06 //EL ALUMBRADO QUE TIENE ESTA VIVIENDA ES
    {
        [Key]
        public decimal CodS03P06 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P06 { get; set; }
    }
}
