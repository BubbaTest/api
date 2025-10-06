using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace Alexa.Middleware //WebApiUsuarios
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
        private readonly RequestDelegate _siguiente;
        private readonly ILogger<LogueaRespuestaErrorHTTPMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public LogueaRespuestaErrorHTTPMiddleware(
            RequestDelegate siguiente,
            ILogger<LogueaRespuestaErrorHTTPMiddleware> logger,
            IWebHostEnvironment env)
        {
            _siguiente = siguiente;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext contexto)
        {
            if (!contexto.Request.Path.StartsWithSegments("/endpoint/cipc") &&
                !contexto.Request.Path.StartsWithSegments("/endpoint/cipp"))
            {
                await _siguiente(contexto);
                return;
            }

            using var ms = new MemoryStream();
            var cuerpoOriginalRespuesta = contexto.Response.Body;
            contexto.Response.Body = ms;

            bool huboExcepcion = false;

            try
            {
                await _siguiente(contexto);
            }
            catch (Exception ex)
            {
                contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await RegistrarErrorEnArchivo(contexto, ex);
                huboExcepcion = true;
            }

            ms.Seek(0, SeekOrigin.Begin);
            string respuesta = await new StreamReader(ms).ReadToEndAsync();
            ms.Seek(0, SeekOrigin.Begin);

            await ms.CopyToAsync(cuerpoOriginalRespuesta);
            contexto.Response.Body = cuerpoOriginalRespuesta;

            // Registrar solo si hay error y no hubo excepción (para evitar doble registro)
            if (contexto.Response.StatusCode >= 400 && !huboExcepcion)
            {
                await RegistrarErrorEnArchivo(contexto, respuesta);
            }
        }

        //public async Task InvokeAsync(HttpContext contexto)
        //{
        //    // Verificar si la ruta pertenece a CipcController o CippController
        //    if (!contexto.Request.Path.StartsWithSegments("/endpoint/cipc") &&
        //        !contexto.Request.Path.StartsWithSegments("/endpoint/cipp"))
        //    {
        //        await _siguiente(contexto);
        //        return;
        //    }

        //    using var ms = new MemoryStream();
        //    var cuerpoOriginalRespuesta = contexto.Response.Body;
        //    contexto.Response.Body = ms;

        //    try
        //    {
        //        await _siguiente(contexto);
        //    }
        //    catch (Exception ex)
        //    {
        //        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //        await RegistrarErrorEnArchivo(contexto, ex);
        //    }

        //    ms.Seek(0, SeekOrigin.Begin);
        //    string respuesta = await new StreamReader(ms).ReadToEndAsync();
        //    ms.Seek(0, SeekOrigin.Begin);

        //    await ms.CopyToAsync(cuerpoOriginalRespuesta);
        //    contexto.Response.Body = cuerpoOriginalRespuesta;

        //    // Registrar solo si hay error y es de CipcController o CippController
        //    if (contexto.Response.StatusCode >= 400)
        //    {
        //        await RegistrarErrorEnArchivo(contexto, respuesta);
        //    }
        //}

        private async Task RegistrarErrorEnArchivo(HttpContext contexto, Exception ex)
        {
            try
            {
                string rutaAuditoria = Path.Combine(_env.WebRootPath, "errores.txt");

                var sb = new StringBuilder();

                sb.AppendLine($"===== ERROR {contexto.Response.StatusCode} =====");
                sb.AppendLine($"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Ruta: {contexto.Request.Path}");
                sb.AppendLine($"Método: {contexto.Request.Method}");
                sb.AppendLine("Mensaje de error:");
                sb.AppendLine(ex.Message);
                sb.AppendLine();

                // Agregar detalles de InnerException recursivamente
                sb.AppendLine("Detalles de InnerException:");
                sb.AppendLine(ObtenerDetallesInnerException(ex.InnerException));

                sb.AppendLine(new string('=', 50));
                sb.AppendLine();

                await File.AppendAllTextAsync(rutaAuditoria, sb.ToString());
                _logger.LogWarning("Error {0} registrado con contexto de solicitud", contexto.Response.StatusCode);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error al registrar fallo");
            }
        }

        private string ObtenerDetallesInnerException(Exception innerEx, int nivel = 1)
        {
            if (innerEx == null)
                return "Ninguna";

            var sb = new StringBuilder();

            string indent = new string('\t', nivel);

            sb.AppendLine($"{indent}Nivel {nivel}: {innerEx.Message}");
            if (!string.IsNullOrEmpty(innerEx.StackTrace))
            {
                sb.AppendLine($"{indent}StackTrace:");
                sb.AppendLine($"{indent}{innerEx.StackTrace.Replace("\n", "\n" + indent)}");
            }

            if (innerEx.InnerException != null)
            {
                sb.AppendLine(ObtenerDetallesInnerException(innerEx.InnerException, nivel + 1));
            }

            return sb.ToString();
        }

        //private async Task RegistrarErrorEnArchivo(HttpContext contexto, Exception ex)
        //{
        //    try
        //    {
        //        string rutaAuditoria = Path.Combine(_env.WebRootPath, "errores.txt");

        //        string registro = $"===== ERROR {contexto.Response.StatusCode} =====\n" +
        //                          $"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
        //                          $"Ruta: {contexto.Request.Path}\n" +
        //                          $"Método: {contexto.Request.Method}\n" +
        //                          $"Mensaje de error: {ex.Message}\n";

        //        // Capturar InnerException si existe
        //        if (ex.InnerException != null)
        //        {
        //            registro += $"Inner Exception: {ex.InnerException.Message}\n";
        //        }

        //        await File.AppendAllTextAsync(rutaAuditoria, registro);
        //        _logger.LogWarning("Error {0} registrado con contexto de solicitud", contexto.Response.StatusCode);
        //    }
        //    catch (Exception logEx)
        //    {
        //        _logger.LogError(logEx, "Error al registrar fallo");
        //    }
        //}

        private async Task RegistrarErrorEnArchivo(HttpContext contexto, string respuesta)
        {
            try
            {
                string rutaAuditoria = Path.Combine(_env.WebRootPath, "errores.txt");

                // Limitar tamaño de respuesta para logs (3000 caracteres)
                string respuestaLog = respuesta.Length > 3000 ? respuesta.Substring(0, 3000) + "..." : respuesta;

                string registro = $"===== ERROR {contexto.Response.StatusCode} =====\n" +
                                  $"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                  $"Ruta: {contexto.Request.Path}\n" +
                                  $"Método: {contexto.Request.Method}\n" +
                                  $"Respuesta: {respuestaLog}\n\n";

                await File.AppendAllTextAsync(rutaAuditoria, registro);
                _logger.LogWarning("Error {0} registrado con contexto de solicitud", contexto.Response.StatusCode);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "Error al registrar fallo");
            }
        }


        //public async Task InvokeAsync(HttpContext contexto)
        //{
        //    // Verificar si la ruta pertenece a CipcController o CippController
        //    if (!contexto.Request.Path.StartsWithSegments("/endpoint/cipc") &&
        //        !contexto.Request.Path.StartsWithSegments("/endpoint/cipp"))
        //    {
        //        await _siguiente(contexto);
        //        return;
        //    }

        //    using var ms = new MemoryStream();
        //    var cuerpoOriginalRespuesta = contexto.Response.Body;
        //    contexto.Response.Body = ms;

        //    try
        //    {
        //        await _siguiente(contexto);
        //    }
        //    catch (Exception ex)
        //    {
        //        contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //        _logger.LogError(ex, "Error en controlador");
        //    }

        //    ms.Seek(0, SeekOrigin.Begin);
        //    string respuesta = await new StreamReader(ms).ReadToEndAsync();
        //    ms.Seek(0, SeekOrigin.Begin);

        //    await ms.CopyToAsync(cuerpoOriginalRespuesta);
        //    contexto.Response.Body = cuerpoOriginalRespuesta;

        //    // Registrar solo si hay error y es de CipcController o CippController
        //    if (contexto.Response.StatusCode >= 400)
        //    {
        //        await RegistrarErrorEnArchivo(contexto, respuesta);
        //    }
        //}

        //private async Task RegistrarErrorEnArchivo(HttpContext contexto, string respuesta)
        //{
        //    try
        //    {
        //        string rutaAuditoria = Path.Combine(_env.WebRootPath, "errores.txt");

        //        string registro = "===== ERROR " + contexto.Response.StatusCode + " =====\n" +
        //                        "Fecha: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
        //                        "Ruta: " + contexto.Request.Path + "\n" +
        //                        "Método: " + contexto.Request.Method + "\n";

        //        // Limitar tamaño de respuesta para logs (3000 caracteres)
        //        string respuestaLog = respuesta.Length > 3000 ? respuesta.Substring(0, 3000) + "..." : respuesta;
        //        registro += "Respuesta: " + respuestaLog + "\n\n";

        //        //// Agregar información del contexto de la solicitud
        //        //registro += "Encabezados:\n";
        //        //foreach (var header in contexto.Request.Headers)
        //        //{
        //        //    registro += $"{header.Key}: {header.Value}\n";
        //        //}

        //        //registro += "Parámetros de consulta:\n";
        //        //foreach (var query in contexto.Request.Query)
        //        //{
        //        //    registro += $"{query.Key}: {query.Value}\n";
        //        //}

        //        //// Leer el cuerpo de la solicitud (si es posible)
        //        //contexto.Request.EnableBuffering(); // Permitir que el cuerpo se lea varias veces
        //        //using (var reader = new StreamReader(contexto.Request.Body, Encoding.UTF8, leaveOpen: true))
        //        //{
        //        //    var cuerpoSolicitud = await reader.ReadToEndAsync();
        //        //    contexto.Request.Body.Position = 0; // Reiniciar la posición del flujo
        //        //    registro += "Cuerpo de la solicitud:\n" + cuerpoSolicitud + "\n";
        //        //}

        //        //registro += "\n";

        //        await File.AppendAllTextAsync(rutaAuditoria, registro);
        //        _logger.LogWarning("Error {0} registrado con contexto de solicitud", contexto.Response.StatusCode);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al registrar fallo");
        //    }
        //}


        //    public async Task InvokeAsync(HttpContext contexto)
        //    {
        //        // Verificar si la ruta pertenece a CipcController
        //        if (!contexto.Request.Path.StartsWithSegments("/endpoint/cipc"))
        //        {
        //            await _siguiente(contexto);
        //            return;
        //        }

        //        using var ms = new MemoryStream();
        //        var cuerpoOriginalRespuesta = contexto.Response.Body;
        //        contexto.Response.Body = ms;

        //        try
        //        {
        //            await _siguiente(contexto);
        //        }
        //        catch (Exception ex)
        //        {
        //            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //            _logger.LogError(ex, "Error en CipcController");
        //        }

        //        ms.Seek(0, SeekOrigin.Begin);
        //        string respuesta = await new StreamReader(ms).ReadToEndAsync();
        //        ms.Seek(0, SeekOrigin.Begin);

        //        await ms.CopyToAsync(cuerpoOriginalRespuesta);
        //        contexto.Response.Body = cuerpoOriginalRespuesta;

        //        // Registrar solo si hay error y es de CipcController
        //        if (contexto.Response.StatusCode >= 400)
        //        {
        //            await RegistrarErrorEnArchivo(contexto, respuesta);
        //        }
        //    }

        //    private async Task RegistrarErrorEnArchivo(HttpContext contexto, string respuesta)
        //    {
        //        try
        //        {
        //            string rutaAuditoria = Path.Combine(_env.WebRootPath, "CipcErrores.txt");

        //            string registro = "===== ERROR CIPC " + contexto.Response.StatusCode + " =====\n" +
        //                            "Fecha: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
        //                            "Ruta: " + contexto.Request.Path + "\n" +
        //                            "Método: " + contexto.Request.Method + "\n";

        //            // Limitar tamaño de respuesta para logs (300 caracteres)
        //            string respuestaLog = respuesta.Length > 300 ? respuesta.Substring(0, 300) + "..." : respuesta;
        //            registro += "Respuesta: " + respuestaLog + "\n\n";

        //            await File.AppendAllTextAsync(rutaAuditoria, registro);
        //            _logger.LogWarning("Error {0} registrado en CipcController", contexto.Response.StatusCode);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error al registrar fallo de CipcController");
        //        }
        //    }
        //}


        //public static class LogueaRespuestaHTTpMiddlewareExtension
        //{
        //    public static IApplicationBuilder UseLogueaRespuestaHTTp(this IApplicationBuilder app)
        //    {
        //        return app.UseMiddleware<LogueaRespuestaHTTpMiddleware>();
        //    }
        //}

        //public class LogueaRespuestaHTTpMiddleware
        //{
        //    private readonly RequestDelegate _siguiente;
        //    private readonly ILogger<LogueaRespuestaHTTpMiddleware> _logger;
        //    private readonly IWebHostEnvironment _env;

        //    public LogueaRespuestaHTTpMiddleware(
        //        RequestDelegate siguiente,
        //        ILogger<LogueaRespuestaHTTpMiddleware> logger,
        //        IWebHostEnvironment env)
        //    {
        //        _siguiente = siguiente;
        //        _logger = logger;
        //        _env = env;
        //    }

        //    public async Task InvokeAsync(HttpContext contexto)
        //    {
        //        // Capturar la respuesta
        //        using var ms = new MemoryStream();
        //        var cuerpoOriginalRespuesta = contexto.Response.Body;
        //        contexto.Response.Body = ms;

        //        await _siguiente(contexto);

        //        // Leer la respuesta
        //        ms.Seek(0, SeekOrigin.Begin);
        //        string respuesta = await new StreamReader(ms).ReadToEndAsync();
        //        ms.Seek(0, SeekOrigin.Begin);

        //        // Restaurar el stream original
        //        await ms.CopyToAsync(cuerpoOriginalRespuesta);
        //        contexto.Response.Body = cuerpoOriginalRespuesta;

        //        // Registrar en archivo
        //        try
        //        {
        //            string rutaAuditoria = Path.Combine(_env.WebRootPath, "Auditoria.txt");
        //            string registro = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}]\nRespuesta: {respuesta}\n\n";

        //            await File.AppendAllTextAsync(rutaAuditoria, registro);
        //            _logger.LogInformation("Respuesta registrada en {ruta}", rutaAuditoria);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error al registrar la respuesta en archivo");
        //        }
        //    }
        //}


        //public class LogueaRespuestaHTTpMiddleware2
        //{
        //    private readonly RequestDelegate siguiente;
        //    private readonly ILogger<LogueaRespuestaHTTpMiddleware2> logger;

        //    public LogueaRespuestaHTTpMiddleware2(RequestDelegate siguiente,
        //            ILogger<LogueaRespuestaHTTpMiddleware2> logger
        //        )
        //    {
        //        this.siguiente = siguiente;
        //        this.logger = logger;
        //    }

        //    //Invoke o InvokeAsync
        //    public async Task InvokeAsync(HttpContext contexto)
        //    {
        //        using var ms = new MemoryStream();
        //        var cuerpoOriginalRespuesta = contexto.Response.Body;
        //        contexto.Response.Body = ms;

        //        await siguiente(contexto);
        //        ms.Seek(0, SeekOrigin.Begin);
        //        string respuesta = new StreamReader(ms).ReadToEnd();
        //        ms.Seek(0, SeekOrigin.Begin);

        //        await ms.CopyToAsync(cuerpoOriginalRespuesta);
        //        contexto.Response.Body = cuerpoOriginalRespuesta;

        //        logger.LogInformation(respuesta);
        //    }
        //}

        //public static class LogueaRespuestaErrorHTTPMiddlewareExtension
        //{
        //    public static IApplicationBuilder UseLogueaRespuestaErrorHTTP(this IApplicationBuilder app)
        //    {
        //        return app.UseMiddleware<LogueaRespuestaErrorHTTPMiddleware>();
        //    }
        //}

        //public class LogueaRespuestaErrorHTTPMiddleware
        //{
        //    private readonly RequestDelegate _siguiente;
        //    private readonly ILogger<LogueaRespuestaErrorHTTPMiddleware> _logger;
        //    private readonly IWebHostEnvironment _env;

        //    public LogueaRespuestaErrorHTTPMiddleware(
        //        RequestDelegate siguiente,
        //        ILogger<LogueaRespuestaErrorHTTPMiddleware> logger,
        //        IWebHostEnvironment env)
        //    {
        //        _siguiente = siguiente;
        //        _logger = logger;
        //        _env = env;
        //    }

        //    public async Task InvokeAsync(HttpContext contexto)
        //    {
        //        using var ms = new MemoryStream();
        //        var cuerpoOriginalRespuesta = contexto.Response.Body;
        //        contexto.Response.Body = ms;

        //        try
        //        {
        //            await _siguiente(contexto);
        //        }
        //        catch (Exception ex)
        //        {
        //            // Solo capturamos la excepción para procesar después
        //            contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
        //            _logger.LogError(ex, "Error durante la ejecución");
        //        }

        //        ms.Seek(0, SeekOrigin.Begin);
        //        string respuesta = await new StreamReader(ms).ReadToEndAsync();
        //        ms.Seek(0, SeekOrigin.Begin);

        //        // Restaurar el stream original
        //        await ms.CopyToAsync(cuerpoOriginalRespuesta);
        //        contexto.Response.Body = cuerpoOriginalRespuesta;

        //        // Registrar solo si hay error (400-599)
        //        if (contexto.Response.StatusCode >= 400)
        //        {
        //            await RegistrarErrorEnArchivo(contexto, respuesta);
        //        }
        //    }

        //    private async Task RegistrarErrorEnArchivo(HttpContext contexto, string respuesta)
        //    {
        //        try
        //        {
        //            string rutaAuditoria = Path.Combine(_env.WebRootPath, "ErroresAuditoria.txt");

        //            string registro = "===== ERROR " + contexto.Response.StatusCode + " =====\n" +
        //                             "Fecha: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
        //                             "Ruta: " + contexto.Request.Path + "\n" +
        //                             "Método: " + contexto.Request.Method + "\n" +
        //                             "Respuesta: " + respuesta + "\n\n";

        //            await File.AppendAllTextAsync(rutaAuditoria, registro);
        //            _logger.LogWarning("Error HTTP {statusCode} registrado", contexto.Response.StatusCode);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "Error al registrar fallo en archivo");
        //        }
        //    }
        //}

    }
}
