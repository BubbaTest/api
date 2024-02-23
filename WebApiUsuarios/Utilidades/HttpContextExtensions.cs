using Microsoft.EntityFrameworkCore;
namespace Alexa.Utilidades
{
    public static class HttpContextExtensions
    {
        public async static Task InsertarParametrosPaginacionEncabecera<T>(this HttpContext httpContext,
            IQueryable<T> queryable)
        {
            if (httpContext == null) { throw new ArgumentNullException(nameof(httpContext)); }

            double cantidad = await queryable.CountAsync();
            httpContext.Response.Headers.Add("cantidadTotalResgistros", cantidad.ToString());
        }

        public async static Task InsertarParametrosPaginacion<T>(this HttpContext httpContext,
            IQueryable<T> queryable, int cantidadRegistrosPorPagina)
        {
            double cantidad = await queryable.CountAsync();
            double cantidadPaginas = Math.Ceiling(cantidad / cantidadRegistrosPorPagina);
            httpContext.Response.Headers.Add("cantidadPaginas", cantidadPaginas.ToString());
        }
    }
}
