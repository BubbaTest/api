namespace Alexa.DTOs
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;
        private int cantidadResgistrosPorPagina = 10;
        private readonly int cantidadMaximaRegistrosPorPagina = 50;

        public int CantidadRegistrosPorPagina
        {
            get => cantidadResgistrosPorPagina;
            set
            {
                cantidadResgistrosPorPagina = (value > cantidadMaximaRegistrosPorPagina) ? cantidadMaximaRegistrosPorPagina : value;
            }
        }
    }
}
