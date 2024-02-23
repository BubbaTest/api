﻿using System.ComponentModel.DataAnnotations;
namespace Alexa.DAL.Catalogos.CaracteristicasPersonas
{
    public class CatS06P35 //DONDE LE ATENDIERON EL PARTO DEL ULTIMO HIJO O HIJA NACIDO VIVO
    {
        [Key]
        public decimal CodS06P35 { get; set; }
        [Required(ErrorMessage = "el campo {0} es requerido")]
        [StringLength(maximumLength: 100, ErrorMessage = "el campo {0} su longiud es de {1}")]
        public string NomS06P35 { get; set; }
    }
}
