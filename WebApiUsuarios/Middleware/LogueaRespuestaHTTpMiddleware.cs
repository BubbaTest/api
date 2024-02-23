using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace Alexa.Middleware //WebApiUsuarios
{
    public static class LogueaRespuestaHTTpMiddlewareExtension
    {
        public static IApplicationBuilder UseLogueaRespuestaHTTp(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogueaRespuestaHTTpMiddleware>();
        }
    }

    public class LogueaRespuestaHTTpMiddleware
    {
        private readonly RequestDelegate siguiente;
        private readonly ILogger<LogueaRespuestaHTTpMiddleware> logger;

        public LogueaRespuestaHTTpMiddleware(RequestDelegate siguiente,
                ILogger<LogueaRespuestaHTTpMiddleware> logger
            )
        {
            this.siguiente = siguiente;
            this.logger = logger;
        }

        //Invoke o InvokeAsync
        public async Task InvokeAsync(HttpContext contexto)
        {
            using var ms = new MemoryStream();
            var cuerpoOriginalRespuesta = contexto.Response.Body;
            contexto.Response.Body = ms;

            await siguiente(contexto);
            ms.Seek(0, SeekOrigin.Begin);
            string respuesta = new StreamReader(ms).ReadToEnd();
            ms.Seek(0, SeekOrigin.Begin);

            await ms.CopyToAsync(cuerpoOriginalRespuesta);
            contexto.Response.Body = cuerpoOriginalRespuesta;

            logger.LogInformation(respuesta);
        }
    }
}
