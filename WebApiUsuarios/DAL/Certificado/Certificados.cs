using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.Certificado
{
    public class Certificados
    {
        [Key]
        public string N { get; set; }
        public string Departamento { get; set; }
        public string Municipio { get; set; }
        public string NombresApellidos { get; set; }
        public string Cargo { get; set; }
        public string FechaIngreso { get; set; }
        public string FechaBaja { get; set; }
        public string CedulaCorrecta { get; set; }
        public string Tipo { get; set; }
        public bool Activo { get; set; }
    }
}
