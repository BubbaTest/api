using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos.IdentificacionUbicacionGeografica
{
    public class CatS01AP12 //Localidad
    {
        [Key]
        public decimal CodS01AP12 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS01AP12 { get; set; }
    }
}
