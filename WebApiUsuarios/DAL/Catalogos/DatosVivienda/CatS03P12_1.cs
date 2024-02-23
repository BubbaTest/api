using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P12_1 //CUANTO PAGA AL MES POR CONCEPTO DE ALQUILER
    {
        [Key]
        public decimal CodS03P12_1 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P12_1 { get; set; }
    }
}
