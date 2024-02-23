using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Catalogos.DatosVivienda
{
    public class CatS03P02 //EN ESTA VIVIENDA SE REALIZA ALGUNA ACTIVIDAD ECONOMICA
    {
        [Key]
        public decimal CodS03P02 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS03P02 { get; set; }

    }
}
