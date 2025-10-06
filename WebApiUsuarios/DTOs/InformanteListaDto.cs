using System.Diagnostics.Tracing;

namespace Alexa.DTOs
{
    public class InformanteListaDto
    {
        public string CodInformante { get; set; } = null!;

        public string NombreInformante { get; set; } = null!;
        public bool Activo { get; set; }
        public int? Semana { get; set; }
        public string Dia { get; set; } = null!;
    }
}
