using Microsoft.Extensions.Logging;
using System.Text;

namespace Alexa.Middleware
{
    public static class LogueaRespuestaErrorHTTPMiddlewareExtension
    {
        public static IApplicationBuilder UseLogueaRespuestaErrorHTTP(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LogueaRespuestaErrorHTTPMiddleware>();
        }
    }

    public class LogueaRespuestaErrorHTTPMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogueaRespuestaErrorHTTPMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public LogueaRespuestaErrorHTTPMiddleware(
            RequestDelegate next,
            ILogger<LogueaRespuestaErrorHTTPMiddleware> logger,
            IWebHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Only log for specific endpoints as per original logic
            if (!context.Request.Path.StartsWithSegments("/endpoint/cipc") &&
                !context.Request.Path.StartsWithSegments("/endpoint/cipp"))
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                if (context.Response.StatusCode >= 400)
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    string responseText = await new StreamReader(responseBody).ReadToEndAsync();
                    responseBody.Seek(0, SeekOrigin.Begin);

                    await RegistrarErrorEnArchivo(context, responseText);
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await RegistrarErrorEnArchivo(context, ex);
                throw; 
            }
            finally
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;
            }
        }

        private async Task RegistrarErrorEnArchivo(HttpContext context, Exception ex)
        {
            try
            {
                string logsDir = Path.Combine(_env.ContentRootPath, "logs");
                Directory.CreateDirectory(logsDir);
                string logPath = Path.Combine(logsDir, "errores.txt");

                var sb = new StringBuilder();
                sb.AppendLine($"===== EXCEPTION {context.Response.StatusCode} =====");
                sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Ruta: {context.Request.Path}");
                sb.AppendLine($"Método: {context.Request.Method}");
                sb.AppendLine("Mensaje:");
                sb.AppendLine(ex.Message);
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    sb.AppendLine("Inner Exception:");
                    sb.AppendLine(ObtenerDetallesInnerException(ex.InnerException));
                }

                sb.AppendLine(new string('=', 50));
                sb.AppendLine();

                await File.AppendAllTextAsync(logPath, sb.ToString());
                _logger.LogWarning("Excepción registrada en errores.txt");
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error al escribir en el archivo de logs");
            }
        }

        private async Task RegistrarErrorEnArchivo(HttpContext context, string responseBody)
        {
            try
            {
                string logsDir = Path.Combine(_env.ContentRootPath, "logs");
                Directory.CreateDirectory(logsDir);
                string logPath = Path.Combine(logsDir, "errores.txt");

                string responseSnippet = responseBody.Length > 3000 ? responseBody.Substring(0, 3000) + "..." : responseBody;

                var sb = new StringBuilder();
                sb.AppendLine($"===== ERROR HTTP {context.Response.StatusCode} =====");
                sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Ruta: {context.Request.Path}");
                sb.AppendLine($"Método: {context.Request.Method}");
                sb.AppendLine("Respuesta:");
                sb.AppendLine(responseSnippet);
                sb.AppendLine(new string('=', 50));
                sb.AppendLine();

                await File.AppendAllTextAsync(logPath, sb.ToString());
                _logger.LogWarning("Error HTTP {0} registrado en errores.txt", context.Response.StatusCode);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error al escribir en el archivo de logs");
            }
        }

        private string ObtenerDetallesInnerException(Exception innerEx, int nivel = 1)
        {
            if (innerEx == null) return string.Empty;

            var sb = new StringBuilder();
            string indent = new string('\t', nivel);

            sb.AppendLine($"{indent}Nivel {nivel}: {innerEx.Message}");
            if (innerEx.InnerException != null)
            {
                sb.AppendLine(ObtenerDetallesInnerException(innerEx.InnerException, nivel + 1));
            }

            return sb.ToString();
        }

        public string DecodeFrom64(string cadena)
        {
            return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(cadena));
        }

        public string jsonRetorno(int Retorno, string Mensaje, bool Resultado, string Valor = "NINGUNO")
        {
            var jsonMensaje = "{" + "\"" + "Mensaje" + "\"" + ": " + "\"" + Mensaje + "\"" + ", " + "\"" + "Retorno" + "\"" + ": " + "\"" + Retorno.ToString() + "\"" + ", " + "\"" + "Resultado" + "\"" + ": " + "\"" + Resultado.ToString().ToLower() + "\"" + ", " + "\"" + "Valor" + "\"" + ": " + Valor + "}";
            return jsonMensaje;
        }
    }
}
