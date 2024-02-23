using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.DefuncionesHogar
{
    public class CatS08P07 //MURIO DURANTE
    {
        [Key]
        public decimal CodS08P07 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS08P07 { get; set; }
    }
}
