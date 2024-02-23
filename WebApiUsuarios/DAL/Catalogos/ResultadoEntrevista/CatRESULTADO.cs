using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.ResultadoEntrevista
{
    public class CatRESULTADO //RESULTADO DE LA ENTREVISTA
    {
        [Key]
        public decimal CodRESULTADO { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomRESULTADO { get; set; }
    }
}
