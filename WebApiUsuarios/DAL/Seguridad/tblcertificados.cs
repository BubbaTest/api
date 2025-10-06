using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata.Ecma335;

namespace Alexa.DAL.Seguridad
{
    public class tblcertificados
    {
        public string Departamento { get; set; }
        public string Municipio { get; set; }
        public string NombreArchivo { get; set; }
        public string Ruta { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string id { get; set; }
        public Nullable<bool> Activo { get; set; }
    }
}
