using Alexa.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Data.Common;
using Newtonsoft.Json;
using Alexa.DAL.Seguridad;
using System.Security.Cryptography;

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

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter(string utilisatrice, string passe)
        {

            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.UsuarioId == utilisatrice && x.Password == passe && x.Activo == true);

            if (usuario == null)
            {
                return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas", URL = "error" });
            }
            else
            {
                var sToken = ConstruirToken(utilisatrice);
                var olista = new List<object>();
                DbCommand cmd;
                DbDataReader rdr;
                string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @VIN";
                List<SqlParameter> parms = new List<SqlParameter>
                { 
                    // Create parameters    
                    new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
                    new SqlParameter { ParameterName = "@Password", Value = passe },
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

        [HttpGet("Boleta/{construccion}")]
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
                    Response.Headers["Cache-Control"] = "no-store";
                    Response.Headers["Pragma"] = "no-cache";

                    // Devolver una respuesta con el HTML generado
                    return Ok(htmlResult);
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

        private RespuestaAutenticacion ConstruirToken(string utilisatrice)
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
