using Alexa.DTOs;

namespace Alexa.Utilidades
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable,
            PaginacionDTO paginacionDTO)
        {
           
            return queryable
                .Skip((paginacionDTO.Pagina - 1) * paginacionDTO.CantidadRegistrosPorPagina)
                .Take(paginacionDTO.CantidadRegistrosPorPagina);
        }
    }
}
