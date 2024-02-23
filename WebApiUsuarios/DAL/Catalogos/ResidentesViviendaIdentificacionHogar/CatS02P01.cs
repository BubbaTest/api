using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.ResidentesViviendaIdentificacionHogar
{
    public class CatS02P01 //CondicionOcupacion
    {
        [Key]
        public decimal CodS02P01 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS02P01 { get; set; }
    }
}
