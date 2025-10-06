namespace Alexa.DAL.IPC
{
    public class SeriesPrecios
    {
        public string InformanteId { get; set; } = null!;
        public string VariedadId { get; set; } = null!;
        public int? Anio { get; set; }
        public int? Mes { get; set; }
        public string muestraid { get; set; } = null!; // NOT NULL
        public int? Semana { get; set; }
        public DateTime Fecha { get; set; }
        public decimal? PrecioRecolectado { get; set; }
        //public decimal? PrecioAnterior { get; set; }
        public decimal? Peso { get; set; }
        public int? Cantidad { get; set; }
        public string? UnidadMedidaId { get; set; }
        public bool? EsOferta { get; set; }
        public bool? TieneDescuento { get; set; }
        public decimal? Descuento { get; set; }
        public bool? TieneIVA { get; set; }
        public bool? TienePropina { get; set; }
        public int? MonedaId { get; set; }
        public int? EstadoProductoId { get; set; }
        public decimal? PrecioSustituidoR { get; set; }
        public decimal? PrecioSustituidoC { get; set; }
        public string? ObservacionEnumerador { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string? CreadoPor { get; set; }
    }
}
