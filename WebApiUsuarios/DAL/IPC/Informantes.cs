using System.ComponentModel.DataAnnotations;

namespace Alexa.DAL.IPC
{
    public class Informantes
    {
        [Key]

        public string CodInformante { get; set; } = null!;

        public string NombreInformante { get; set; } = null!;
        public string? Direccion { get; set; } // permite NULL
        public int? DistritoId { get; set; }
        public bool Activo { get; set; }
        public double? CoordenadaX { get; set; }
        public double? CoordenadaY { get; set; }
        public string IdEmpleado { get; set; } = null!;
    }
}
