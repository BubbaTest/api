namespace Alexa.DTOs
{
    public class MuestraPreResponseDto
    {
        public List<ObservacionPreviaDto> ObservacionesPrevias { get; set; } = new();
        public List<EstadoCausalDto> EstadosCausales { get; set; } = new();
    }
}
