using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos.IdentificacionUbicacionGeografica
{
    public class CatS01AP07 //Distrito
    {
        [Key]
        public decimal CodS01AP07 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS01AP07 { get; set; }
    }
}
