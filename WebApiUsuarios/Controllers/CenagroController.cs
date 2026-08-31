using Alexa.DAL.Certificado;
using Alexa.DAL.Seguridad;
using Alexa.DTOs;
using Alexa.Filters;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/cenagro")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CenagroController : ControllerBase
    {
        private readonly CenagroDbContext context;
        private readonly IConfiguration configuration;

        public CenagroController(CenagroDbContext context,
            IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter(string utilisatrice, string passe)
        {

            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.UsuarioId == utilisatrice && x.Password == passe && x.Activo == true);

            if (usuario == null)
            {
                return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas", URL = "error" });
            }

            // 3. Generar token
            var sToken = ConstruirToken(utilisatrice);

            // 4. Ejecutar procedimiento almacenado sde.spUsuarioValidar
            var olista = new List<object>();

            using (var cmd = context.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @sistema, @session, @ip, @VIN";
                cmd.Parameters.Add(new SqlParameter("@UsuarioId", utilisatrice));
                cmd.Parameters.Add(new SqlParameter("@Password", passe));
                cmd.Parameters.Add(new SqlParameter("@VIN", sToken.Token));

                if (cmd.Connection.State != ConnectionState.Open)
                {
                    await cmd.Connection.OpenAsync();
                }

                using (var rdr = await cmd.ExecuteReaderAsync())
                {
                    while (await rdr.ReadAsync())
                    {
                        if (rdr[0] != null && rdr[0] != DBNull.Value)
                        {
                            string jsonString = rdr[0].ToString() ?? "[]";
                            dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
                            olista.Add(catalogos);
                        }
                    }
                }
            }

            return Ok(olista);
            //else
            //{
            //    var sToken = ConstruirToken(utilisatrice);
            //    var olista = new List<object>();
            //    DbCommand cmd;
            //    DbDataReader rdr;
            //    string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @VIN";
            //    List<SqlParameter> parms = new List<SqlParameter>
            //    { 
            //        // Create parameters    
            //        new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
            //        new SqlParameter { ParameterName = "@Password", Value = passe },
            //        new SqlParameter { ParameterName = "@VIN", Value = sToken.Token }
            //    };
            //    cmd = context.Database.GetDbConnection().CreateCommand();
            //    cmd.Parameters.AddRange(parms.ToArray());
            //    cmd.CommandText = sql;

            //    // Open database connection  
            //    context.Database.OpenConnection();

            //    // Create a DataReader  
            //    rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

            //    while (rdr.Read())
            //    {
            //        string jsonString = "";
            //        if (rdr[0] != null)
            //        {
            //            jsonString = rdr[0].ToString() ?? "[]";
            //            dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
            //            olista.Add(catalogos);
            //        }
            //        ;
            //    }
            //    rdr.Close();
            //    return Ok(olista);
            //}
        }

        [HttpPost("RefreshToken")]
        [Authorize]
        public IActionResult Refresh()
        {
            var utilisatrice = User.FindFirst("UTILISATRICE")?.Value;
            if (string.IsNullOrEmpty(utilisatrice)) return Unauthorized();

            var token = ConstruirToken(utilisatrice);
            return Ok(token);
        }

        private RespuestaAutenticacion ConstruirToken(string utilisatrice)
        {
            var claims = new List<Claim>
            {
                new Claim("UTILISATRICE", utilisatrice),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationHours = configuration.GetValue<int>("JWT:ExpiresInHours", 12);
            var expiracion = DateTime.UtcNow.AddHours(expirationHours);

            var securityToken = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        private RespuestaAutenticacion ConstruirToken1(string utilisatrice)
        {
            var claims = new List<Claim>();
            claims = new List<Claim>()
                {
                    new Claim("UTILISATRICE", utilisatrice)
                };

            var llave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["llavejwt"]));
            var creds = new SigningCredentials(llave, SecurityAlgorithms.HmacSha256);

            var expiracion = DateTime.UtcNow.AddHours(12);

            var securityToken = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiracion, signingCredentials: creds);

            return new RespuestaAutenticacion()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(securityToken),
                Expiracion = expiracion
            };
        }

        [HttpPost("Certificado")]
        public async Task<ActionResult<string>> ProcesarJsonYGenerarHtml(string cedula, string tipo)
        {
            var certificado = await context.Certificados.AsNoTracking().FirstOrDefaultAsync(x => x.CedulaCorrecta == cedula && x.Tipo == tipo);
            if (certificado == null)
                return NotFound("No se encontró un certificado con la cédula proporcionada.");
            if (certificado.Activo)
                return StatusCode(StatusCodes.Status403Forbidden, "No tiene permiso para acceder a este certificado.");

            var fechaEnLetras = ObtenerFechaEnLetras(DateTime.Now);

            //de la Dirección de Tecnologías de la Información  {fechaEnLetras}
            var htmlFormateado = $@"
            <h3>A. {certificado.NombresApellidos}</h3>
            <p>Por su invaluable labor como {certificado.Cargo} del municipio de {certificado.Municipio}, departamento de {certificado.Departamento}</p>               
            <p>en el V Censo Nacional Agropecuario (V CENAGRO) durante el período {certificado.FechaIngreso} al {certificado.FechaBaja}</p>
            <p>Dado en la ciudad de Managua a los 15 días de mes de Julio del 2025</p>            
            <input type=""hidden"" id=""hiddencodigo"" value=""{certificado.N}"">
            <input type=""hidden"" id=""nomcomp"" value=""{certificado.NombresApellidos}"">";

            return Ok(htmlFormateado.Trim());
        }

        // Método auxiliar para convertir la fecha a texto en español
        private string ObtenerFechaEnLetras(DateTime fecha)
        {
            string dia = NumeroALetras(fecha.Day);
            string mes = fecha.ToString("MMMM", new System.Globalization.CultureInfo("es-ES"));
            int anio = fecha.Year;

            return $"{dia} días del mes de {mes} del {anio}";
        }

        // Método para convertir números a letras (solo para días 1-31)
        private string NumeroALetras(int numero)
        {
            string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve", "diez",
                          "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve", "veinte",
                          "veintiuno", "veintidós", "veintitrés", "veinticuatro", "veinticinco", "veintiséis", "veintisiete", "veintiocho", "veintinueve", "treinta",
                          "treinta y uno" };

            if (numero >= 1 && numero <= 31)
                return unidades[numero];
            else
                return numero.ToString(); // fallback en caso de número fuera de rango
        }

        [HttpPost("Bloquear")]
        public async Task<bool> ActivarCertificadoAsync(int clave)
        {
            var certificado = await context.Certificados.AsNoTracking().FirstOrDefaultAsync(c => c.N == clave);
            if (certificado == null)
                return false; // No se encontró el certificado
            certificado.Activo = true;
            await context.SaveChangesAsync();
            return true; // Actualización exitosa
        }

        [HttpGet("Aprovechamiento")]
        public async Task<IActionResult> Aprovechamiento([FromQuery] string dep, [FromQuery] string mun)
        {
            // Inicializamos con un array JSON vacío por defecto
            string rawJsonResult = "[]";

            var paramRetorno = new SqlParameter("@Retorno", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var paramMensaje = new SqlParameter("@Mensaje", SqlDbType.NVarChar, 1024) { Direction = ParameterDirection.Output };

            try
            {
                var connection = context.Database.GetDbConnection();

                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "[sde].[ListaResultadoAprovechamiento]";
                    command.CommandType = CommandType.StoredProcedure;

                    // Parámetros de Entrada
                    command.Parameters.Add(new SqlParameter("@Dep", SqlDbType.NVarChar, 2) { Value = (object)dep ?? DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@Mun", SqlDbType.NVarChar, 4) { Value = (object)mun ?? DBNull.Value });

                    // Parámetros de Salida
                    command.Parameters.Add(paramRetorno);
                    command.Parameters.Add(paramMensaje);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            // Extraemos directamente el string de la columna "JsonResult"
                            rawJsonResult = reader["JsonResult"]?.ToString() ?? "[]";
                        }
                    }
                }

                // Captura de parámetros de salida
                int retornoVal = paramRetorno.Value != DBNull.Value ? Convert.ToInt32(paramRetorno.Value) : 0;
                string mensajeVal = paramMensaje.Value != DBNull.Value ? paramMensaje.Value.ToString()! : string.Empty;

                if (retornoVal < 0)
                {
                    return BadRequest(new { ErrorCode = retornoVal, Message = mensajeVal });
                }

                // Retornamos el String original con el Header de Content-Type adecuado.
                // Esto evita que .NET intente serializar de nuevo un string que ya es un JSON válido.
                return Content(rawJsonResult, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        [NoCache]
        [HttpGet("Catalogos")]
        public async Task<IActionResult> Catalogos()
        {
            var municipios = await context.MUNICIPIOS.AsNoTracking()              
               .ToListAsync();

            return Ok(municipios);
        }

        private string DecodeFrom64(string cadena)
        {
            return System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(cadena));
        }

        private string jsonRetorno(int Retorno, string Mensaje, bool Resultado, string Valor = "NINGUNO")
        {
            var jsonMensaje = "{" + "\"" + "Mensaje" + "\"" + ": " + "\"" + Mensaje + "\"" + ", " + "\"" + "Retorno" + "\"" + ": " + "\"" + Retorno.ToString() + "\"" + ", " + "\"" + "Resultado" + "\"" + ": " + "\"" + Resultado.ToString().ToLower() + "\"" + ", " + "\"" + "Valor" + "\"" + ": " + Valor + "}";
            return jsonMensaje;
        }

    }
}
