namespace Alexa.DTOs
{
    public class ObservacionPreviaDto
    {
        public int? ObjIdEstablecimientoCanasta { get; set; }
        public int? ObjIdCatVariedad { get; set; }
        public string Observacion { get; set; } = string.Empty;
    }
}
