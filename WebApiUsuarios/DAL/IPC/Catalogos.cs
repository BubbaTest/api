using Alexa.DTOs;

namespace Alexa.DAL.IPC
{
    public class Catalogos
    {
        public List<InformanteListaDto> Informantes { get; set; } = new();
        public List<VariedadesDTO> Variedades { get; set; } = new();
        public List<DiasSemana> DiasSemana { get; set; } = new();
        public List<UmedP> UmedP { get; set; } = new();
        public List<Semana> Semana { get; set; } = new();
        public List<InformanteDto> InformanteDto { get; set; } = new();
    }
}
