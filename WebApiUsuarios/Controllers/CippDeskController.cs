using Alexa.DAL.IPP;
using Alexa.DTOs;
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
    [Route("endpoint/cippdesk")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CippDeskController(IppDeskDbContext context, IConfiguration configuration) : Controller
    {
        [HttpPost("Connecters")]
        public async Task<IActionResult> Connecters(string utilisatrice, string passe)
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var session = BitConverter.ToString(bytes).Replace("-", "");
            var ipAddressString = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var usuario = await context.Usuario.FirstOrDefaultAsync(x => x.UsuarioId == utilisatrice && x.Password == passe && x.Activo == true);

            if (usuario == null)
            {
                return NotFound(new { Retorno = -1, Mensaje = "Credenciales de logueo incorrectas", URL = "error" });
            }

            var sToken = ConstruirToken(utilisatrice);
            var olista = new List<object>();

            string sql = "EXEC sde.spUsuarioValidar @UsuarioId, @Password, @session, @ip, @VIN";
            List<SqlParameter> parms = new List<SqlParameter>
            {
                new SqlParameter { ParameterName = "@UsuarioId", Value = utilisatrice },
                new SqlParameter { ParameterName = "@Password", Value = passe },
                new SqlParameter { ParameterName = "@session", Value = session },
                new SqlParameter { ParameterName = "@ip", Value = ipAddressString },
                new SqlParameter { ParameterName = "@VIN", Value = sToken.Token }
            };

            var connection = context.Database.GetDbConnection();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parms.ToArray());

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            while (await rdr.ReadAsync())
            {
                if (rdr[0] != DBNull.Value)
                {
                    string jsonString = rdr[0].ToString() ?? "[]";
                    var catalogos = JsonConvert.DeserializeObject(jsonString);
                    if (catalogos != null) olista.Add(catalogos);
                }
            }

            return Ok(olista);
        }

        [HttpPost("Connecter")]
        public async Task<IActionResult> Connecter(string utilisatrice, string passe, string sistema)
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var session = BitConverter.ToString(bytes).Replace("-", "");
            var ipAddressString = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

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
                string sql = "EXEC sde.spUsuarioValidar3 @UsuarioId, @Password, @sistema, @session, @ip, @VIN";
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
                        dynamic? catalogos = JsonConvert.DeserializeObject(jsonString);
                        olista.Add(catalogos);
                    }
                    ;
                }
                rdr.Close();
                return Ok(olista);
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

        private string DecodeFrom64(string cadena)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(cadena));
        }

        private string jsonRetorno(int Retorno, string Mensaje, bool Resultado, string Valor = "NINGUNO")
        {
            return JsonConvert.SerializeObject(new { Mensaje, Retorno, Resultado, Valor });
        }
    }
}
