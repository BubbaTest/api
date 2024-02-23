using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CensoEconomico
{
    public class CatRESULTADOEE //RESULTADO DE LA ENTREVISTA CENSO ECONOMICO
    {
        [Key]
        public decimal CodRESULTADOEE { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomRESULTADOEE { get; set; }
    }
}
