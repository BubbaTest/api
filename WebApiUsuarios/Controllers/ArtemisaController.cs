using Alexa.DAL.Seguridad;
using Alexa.DTOs;
using Alexa.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Data;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Alexa.Controllers
{
    [ApiController]
    [Route("endpoint/artemisa")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ArtemisaController : ControllerBase
    {
        private readonly ArtemisaDbContext context;
        private readonly IConfiguration configuration;

        public ArtemisaController(ArtemisaDbContext context,
            IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
        }

        //[HttpPost("Connecter1")]
        //public async Task<IActionResult> Connecter1(string utilisatrice, string passe)
        //{

        //    var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.UsuarioId == utilisatrice && x.Password == passe && x.Activo == true);

        //    if (usuario == null)
        //    {
        //        return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas", URL = "error" });
        //    }

        //    // 3. Generar token
        //    var sToken = ConstruirToken(utilisatrice);

        //    // 4. Ejecutar procedimiento almacenado sde.spUsuarioValidar
        //    var olista = new List<object>();

        //    using (var cmd = context.Database.GetDbConnection().CreateCommand())
        //    {
        //        cmd.CommandText = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @sistema, @session, @ip, @VIN";
        //        cmd.Parameters.Add(new SqlParameter("@UsuarioId", utilisatrice));
        //        cmd.Parameters.Add(new SqlParameter("@Password", passe));
        //        cmd.Parameters.Add(new SqlParameter("@VIN", sToken.Token));

        //        if (cmd.Connection.State != ConnectionState.Open)
        //        {
        //            await cmd.Connection.OpenAsync();
        //        }

        //        using (var rdr = await cmd.ExecuteReaderAsync())
        //        {
        //            while (await rdr.ReadAsync())
        //            {
        //                if (rdr[0] != null && rdr[0] != DBNull.Value)
        //                {
        //                    string jsonString = rdr[0].ToString() ?? "[]";
        //                    dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
        //                    olista.Add(catalogos);
        //                }
        //            }
        //        }
        //    }

        //    return Ok(olista);
        //    //else
        //    //{
        //    //    var sToken = ConstruirToken(utilisatrice);
        //    //    var olista = new List<object>();
        //    //    DbCommand cmd;
        //    //    DbDataReader rdr;
        //    //    string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @VIN";
        //    //    List<SqlParameter> parms = new List<SqlParameter>
        //    //    { 
        //    //        // Create parameters    
        //    //        new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
        //    //        new SqlParameter { ParameterName = "@Password", Value = passe },
        //    //        new SqlParameter { ParameterName = "@VIN", Value = sToken.Token }
        //    //    };
        //    //    cmd = context.Database.GetDbConnection().CreateCommand();
        //    //    cmd.Parameters.AddRange(parms.ToArray());
        //    //    cmd.CommandText = sql;

        //    //    // Open database connection  
        //    //    context.Database.OpenConnection();

        //    //    // Create a DataReader  
        //    //    rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

        //    //    while (rdr.Read())
        //    //    {
        //    //        string jsonString = "";
        //    //        if (rdr[0] != null)
        //    //        {
        //    //            jsonString = rdr[0].ToString() ?? "[]";
        //    //            dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
        //    //            olista.Add(catalogos);
        //    //        }
        //    //        ;
        //    //    }
        //    //    rdr.Close();
        //    //    return Ok(olista);
        //    //}
        //}

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter(string utilisatrice, string passe, string sistema)
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var session = BitConverter.ToString(bytes).Replace("-", "");
            var ipAddressString = HttpContext.Connection.RemoteIpAddress.ToString();

            var usuario = await (from cust in context.Usuario
                                 join ord in context.relUsuarioRol
                                 on new { a = cust.UsuarioId } equals new { a = ord.UsuarioId } into ps
                                 from ord in ps.DefaultIfEmpty()
                                 where cust.UsuarioId == utilisatrice && cust.Password == passe && cust.Activo == true
                                 select cust).ToListAsync();

            if (usuario.Count == 0)
            {
                return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas / Usuario Inactivo o sin Rol", URL = "Usuario/Login" });
            }
            else
            {
                var sToken = ConstruirToken(utilisatrice);
                var olista = new List<object>();
                DbCommand cmd;
                DbDataReader rdr;
                string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @sistema, @session, @ip, @VIN";
                List<SqlParameter> parms = new List<SqlParameter>
                { 
                    // Create parameters    
                    new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
                    new SqlParameter { ParameterName = "@Password", Value = passe },
                    new SqlParameter { ParameterName = "@sistema", Value = sistema },
                    new SqlParameter { ParameterName = "@session", Value = session },
                    new SqlParameter { ParameterName = "@ip", Value = ipAddressString },
                    new SqlParameter { ParameterName = "@VIN", Value = sToken.Token }
                };
                cmd = context.Database.GetDbConnection().CreateCommand();
                cmd.Parameters.AddRange(parms.ToArray());
                cmd.CommandText = sql;

                // Open database connection  
                context.Database.OpenConnection();

                // Create a DataReader  
                rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

                while (rdr.Read())
                {
                    string jsonString = "";
                    if (rdr[0] != null)
                    {
                        jsonString = rdr[0].ToString() ?? "[]";
                        dynamic catalogos = JsonConvert.DeserializeObject(jsonString);
                        olista.Add(catalogos);
                    }
                    ;
                }
                rdr.Close();
                return Ok(olista);
            }
        }

        [NoCache]
        [HttpGet("Boleta/{construccion}")]
        [Produces("text/html")]
        public async Task<IActionResult> Boleta([FromRoute] string construccion)
        {
            DbCommand cmd;

            // Definición del comando SQL con parámetros de entrada
            string sql = "EXEC [dbo].[REPORTEBOLETA] @construccion";

            // Crear el comando
            cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;

            // Agregar parámetros de entrada
            cmd.Parameters.Add(new SqlParameter("@construccion", SqlDbType.NVarChar, 7) { Value = construccion });

            try
            {
                // Abrir la conexión a la base de datos
                await context.Database.OpenConnectionAsync();

                // Ejecutar el comando y obtener el resultado
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    string htmlResult = string.Empty; // Valor predeterminado si no hay datos

                    if (reader.HasRows)
                    {
                        // Leer el resultado devuelto por el procedimiento almacenado
                        await reader.ReadAsync();
                        htmlResult = reader["Resultado"].ToString() ?? string.Empty; // Recuperar el campo "Resultado"
                    }

                    // Cerrar el DataReader para liberar los parámetros de salida
                    reader.Close();

                    // Establecer encabezados para evitar caché
                    //Response.Headers["Cache-Control"] = "no-store";
                    //Response.Headers["Pragma"] = "no-cache";

                    // Devolver una respuesta con el HTML generado
                    // ✅ Retorna explícitamente con tipo de contenido y encoding
                    return Content(htmlResult, "text/html; charset=utf-8");
                    //return Ok(htmlResult);
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(500, $"Error al ejecutar el procedimiento almacenado: {ex.Message}");
            }
            finally
            {
                // Asegurarse de cerrar la conexión
                if (context.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await context.Database.CloseConnectionAsync();
                }
            }
        }
                
        [NoCache]
        [HttpGet("Faltantes/{dep}/{per}/{ar}")] 
        [Produces("text/html")]
        public async Task<IActionResult> Faltantes([FromRoute] string dep,[FromRoute] string per,[FromRoute] int ar,
            [FromQuery] string? br, [FromQuery] int? dis) 
        {
            //[NoCache]
            //[HttpGet("Faltantes/{dep}/{per}/{ar}")]
            //[HttpGet("Faltantes/{dep}/{per}/{ar}/{br}/{dis}")]
            //[Produces("text/html")]
            //public async Task<IActionResult> Faltantes(
            //    string dep,
            //    string per,
            //    int ar,
            //    string? br = "",
            //    int? dis = 0)
            //{

            // ✅ Validaciones básicas de entrada
            if (string.IsNullOrWhiteSpace(dep) || dep.Length > 2)
                return BadRequest("El parámetro 'dep' no es válido.");
            if (string.IsNullOrWhiteSpace(per) || per.Length > 5)
                return BadRequest("El parámetro 'per' no es válido.");
            if (ar < 0)
                return BadRequest("El parámetro 'ar' debe ser positivo.");

            string brString = br ?? string.Empty;
            int disInt = dis ?? 0;
            
            DbCommand cmd;

            // Definición del comando SQL con parámetros de entrada
            string sql = "EXEC [dbo].[REPORTEFALTANTE] @dep, @per, @ar, @br, @dis";

            // Crear el comando
            cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = sql;

            // Agregar parámetros de entrada
            cmd.Parameters.Add(new SqlParameter("@dep", SqlDbType.VarChar, 2) { Value = dep });
            cmd.Parameters.Add(new SqlParameter("@per", SqlDbType.VarChar, 5) { Value = per });
            cmd.Parameters.Add(new SqlParameter("@ar", SqlDbType.VarChar, 1) { Value = ar.ToString() });
            cmd.Parameters.Add(new SqlParameter("@br", SqlDbType.NVarChar, 3) { Value = brString });
            cmd.Parameters.Add(new SqlParameter("@dis", SqlDbType.Int) { Value = disInt });

            try
            {
                // Abrir la conexión a la base de datos
                await context.Database.OpenConnectionAsync();

                // Ejecutar el comando y obtener el resultado
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    string htmlResult = string.Empty; // Valor predeterminado si no hay datos

                    if (reader.HasRows)
                    {
                        // Leer el resultado devuelto por el procedimiento almacenado
                        await reader.ReadAsync();
                        htmlResult = reader["Resultado"].ToString() ?? string.Empty; // Recuperar el campo "Resultado"
                    }

                    // Cerrar el DataReader para liberar los parámetros de salida
                    reader.Close();
                                 

                    // Devolver una respuesta con el HTML generado
                    return Content(htmlResult, "text/html; charset=utf-8");
                    //return Ok(htmlResult);
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return StatusCode(500, $"Error al ejecutar el procedimiento almacenado: {ex.Message}");
            }
            finally
            {
                // Asegurarse de cerrar la conexión
                if (context.Database.GetDbConnection().State == ConnectionState.Open)
                {
                    await context.Database.CloseConnectionAsync();
                }
            }
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
