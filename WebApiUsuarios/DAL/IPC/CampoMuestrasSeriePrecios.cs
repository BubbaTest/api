namespace Alexa.DAL.IPC
{
    public class CampoMuestrasSeriePrecios
    {
        public string InformanteId { get; set; } = null!;

        public string VariedadId { get; set; } = null!;

        public string? Descripcion { get; set; }

        public string? Especificacion { get; set; }

        public string? Detalle { get; set; }

        public DateTime Fecha { get; set; }

        public int? Anio { get; set; }

        public int? Mes { get; set; }

        public int? Semana { get; set; }
        public string? muestraid { get; set; }

        public string? DiaSemanaId { get; set; }

        public int? Nveces { get; set; }

        public bool? EsPesable { get; set; }

        public decimal? PrecioRecolectadoAnt { get; set; }

        public int? CantidadAnt { get; set; }
        public decimal? PesoAnt { get; set; }

        public string? UnidadMedidaId { get; set; }
        public int? MonedaId { get; set; }
        public string? ObservacionAnalista { get; set; }
        public decimal? PrecioCalculado { get; set; }
    }
}
