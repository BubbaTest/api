using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos.IdentificacionUbicacionGeografica
{
    public class CatS01AP02 //Departamentos
    {
        [Key]
        public decimal CodS01AP02 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS01AP02 { get; set; }
        public List<CatS01AP03> CatS01AP03 { get; set; }
    }
}
