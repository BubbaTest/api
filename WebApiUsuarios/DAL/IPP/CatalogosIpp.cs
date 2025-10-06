using Alexa.DTOs;

namespace Alexa.DAL.IPP
{
    public class CatalogosIpp
    {
        public List<CatCalendario> Calendarios { get; set; } = new();
        public List<SEC_MUNIDTO> Muncipios { get; set; } = new();
        public List<CatCanasta> Canasta { get; set; } = new();
        public List<CatValorCatalogo> Causales { get; set; } = new();
        public List<CatValorCatalogo> Estados { get; set; } = new();
        public List<CatValorCatalogo> Monedas { get; set; } = new();
        public List<CatTipoCambio> TipoCambios { get; set; } = new();
        public List<UnidadMedidaDTO> UnidadMedida { get; set; } = new();
    }
}
